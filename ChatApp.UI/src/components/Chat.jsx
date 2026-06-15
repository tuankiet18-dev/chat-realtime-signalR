import { useState, useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import {
  ArrowCounterClockwise,
  ChatCircleDots,
  CheckCircle,
  Circle,
  File,
  ImageSquare,
  PaperPlaneRight,
  SignOut,
  SpinnerGap,
  ThumbsUp,
  Trash,
  UsersThree,
  VideoCamera,
  X,
} from '@phosphor-icons/react';
import { useAuth } from '../contexts/AuthContext';
import { getApiUrl, uploadFile } from '../services/api';

const emojiShortcuts = ['\u{1F44D}', '\u{2764}\u{FE0F}', '\u{1F602}', '\u{1F62E}', '\u{1F525}', '\u{1F389}'];

const formatTime = (value) =>
  new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

const fileSize = (bytes = 0) => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

const initials = (name = '?') => name.trim().charAt(0).toUpperCase() || '?';

export default function Chat() {
  const { user, logout } = useAuth();
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
  const [onlineUsers, setOnlineUsers] = useState([]);
  const [typingUsers, setTypingUsers] = useState({});
  const [composerText, setComposerText] = useState('');
  const [selectedAttachment, setSelectedAttachment] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState(null);
  const messagesContainerRef = useRef(null);
  const messagesEndRef = useRef(null);
  const fileInputRef = useRef(null);
  const typingTimeoutRef = useRef(null);
  const [fullscreenImage, setFullscreenImage] = useState(null);

  const normalizeMessage = useCallback((message) => {
    const isActive = (message.state ?? '').toLowerCase() === 'active';
    return {
      ...message,
      canRecall: isActive && message.senderId === user.id,
      canAdminDelete: isActive && user.role === 'Admin',
    };
  }, [user.id, user.role]);

  const upsertMessage = useCallback((message) => {
    const normalized = normalizeMessage(message);
    setMessages(prev => {
      const index = prev.findIndex(m => m.id === normalized.id);
      if (index >= 0) {
        const updated = [...prev];
        updated[index] = normalized;
        return updated;
      }
      return [...prev, normalized];
    });
  }, [normalizeMessage]);

  useEffect(() => {
    const container = messagesContainerRef.current;
    if (!container) return;

    container.scrollTo({
      top: container.scrollHeight,
      behavior: 'smooth',
    });
  }, [messages]);

  useEffect(() => {
    let activeConnection = null;
    let disposed = false;

    const connect = async () => {
      const token = localStorage.getItem('chat_token');
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${getApiUrl()}/hubs/chat`, {
          accessTokenFactory: () => token,
          withCredentials: false,
        })
        .withAutomaticReconnect()
        .build();
      activeConnection = newConnection;

      newConnection.on('RecentMessagesLoaded', (loaded) => setMessages(loaded.map(normalizeMessage)));

      newConnection.on('MessageReceived', (message) => {
        upsertMessage(message);
        if (message.senderId !== user.id) {
          newConnection.invoke('MarkSeen', message.id).catch(() => {});
        }
      });

      newConnection.on('MessageRecalled', upsertMessage);
      newConnection.on('MessageDeleted', upsertMessage);
      newConnection.on('ReadReceiptsChanged', upsertMessage);
      newConnection.on('PresenceChanged', (users) => setOnlineUsers(users));

      newConnection.on('TypingChanged', (state) => {
        setTypingUsers(prev => {
          const updated = { ...prev };
          if (state.isTyping) {
            updated[state.userId] = state.displayName;
          } else {
            delete updated[state.userId];
          }
          return updated;
        });
      });

      try {
        await newConnection.start();
        if (!disposed) {
          setConnection(newConnection);
          setError(null);
        }
      } catch (err) {
        if (!disposed) {
          setError('Connection failed: ' + err.message);
        }
      }
    };

    connect();
    return () => {
      disposed = true;
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
      if (activeConnection) activeConnection.stop();
    };
  }, [user.id, normalizeMessage, upsertMessage]);

  const clearAttachment = useCallback(() => {
    setSelectedAttachment(null);
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }, [previewUrl]);

  const handleSend = async () => {
    if (!connection || (!composerText.trim() && !selectedAttachment)) return;
    setSending(true);
    setError(null);
    try {
      await connection.invoke('SendMessage', {
        text: composerText,
        attachment: selectedAttachment
      });
      setComposerText('');
      clearAttachment();
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
      await connection.invoke('Typing', false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSending(false);
    }
  };

  const handleTyping = (e) => {
    setComposerText(e.target.value);
    if (!connection) return;

    connection.invoke('Typing', true).catch(() => {});
    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }
    typingTimeoutRef.current = setTimeout(() => {
      connection.invoke('Typing', false).catch(() => {});
    }, 1200);
  };

  const handleFileUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    setError(null);

    if (file.type.startsWith('image/')) {
      const url = URL.createObjectURL(file);
      setPreviewUrl(url);
    } else {
      setPreviewUrl(null);
    }

    try {
      const upload = await uploadFile(file);
      setSelectedAttachment({
        url: upload.url,
        fileName: upload.fileName,
        contentType: upload.contentType,
        size: upload.size,
        kind: upload.kind
      });
    } catch (err) {
      setError(err.message);
      clearAttachment();
    }
  };

  const handleRecall = async (id) => {
    if (!connection) return;
    try {
      await connection.invoke('RecallMessage', id);
    } catch (err) {
      setError(err.message);
    }
  };

  const handleAdminDelete = async (id) => {
    if (!connection) return;
    try {
      await connection.invoke('AdminDeleteMessage', id);
    } catch (err) {
      setError(err.message);
    }
  };

  const sendQuickLike = async () => {
    if (!connection) return;
    setSending(true);
    setError(null);
    try {
      await connection.invoke('SendMessage', {
        text: '\u{1F44D}',
        attachment: null
      });
      await connection.invoke('Typing', false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSending(false);
    }
  };

  const typingNames = Object.values(typingUsers).filter(Boolean).join(', ');
  const connectionReady = Boolean(connection);
  const memberCount = onlineUsers.length;

  return (
    <main className="fixed inset-0 grid grid-cols-1 grid-rows-[minmax(0,1fr)] overflow-hidden bg-[#f4f7fb] text-primary lg:grid-cols-[320px_minmax(0,1fr)]">
      <aside className="hidden min-h-0 overflow-hidden border-r border-line bg-white lg:flex lg:flex-col">
        <div className="border-b border-line p-5">
          <div className="mb-6 flex items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <div className="grid h-11 w-11 place-items-center rounded-2xl bg-accent text-white shadow-lg shadow-blue-200">
                <ChatCircleDots size={24} weight="fill" />
              </div>
              <div>
                <p className="text-[11px] font-black uppercase tracking-[0.18em] text-accent">Shared Room</p>
                <h1 className="text-2xl font-black leading-tight">ChatApp</h1>
              </div>
            </div>
            <button onClick={logout} className="icon-button" title="Dang xuat" aria-label="Dang xuat">
              <SignOut size={21} weight="bold" />
            </button>
          </div>

          <div className="rounded-2xl border border-line bg-slate-50 p-4 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="grid h-12 w-12 place-items-center rounded-full bg-primary text-lg font-black text-white">
                {initials(user.displayName || user.email)}
              </div>
              <div className="min-w-0">
                <p className="truncate text-base font-black">{user.displayName || user.email}</p>
                <p className="text-sm font-semibold text-secondary">{user.role}</p>
              </div>
            </div>
          </div>
        </div>

        <div className="min-h-0 flex-1 overflow-y-auto p-5">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm font-black">
              <UsersThree size={20} weight="fill" className="text-accent" />
              Online
            </div>
            <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-black text-emerald-600">
              {memberCount}
            </span>
          </div>

          <div className="grid gap-2">
            {onlineUsers.map(u => (
              <div key={u.userId} className="flex items-center gap-3 rounded-2xl border border-transparent p-3 transition-colors duration-200 hover:border-line hover:bg-slate-50">
                <div className="relative">
                  <div className="grid h-11 w-11 place-items-center rounded-full bg-blue-50 font-black text-accent">
                    {initials(u.displayName)}
                  </div>
                  <span className="absolute bottom-0 right-0 h-3 w-3 rounded-full border-2 border-white bg-emerald-500" />
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate font-black">{u.displayName}</p>
                  <p className="text-xs font-semibold uppercase tracking-wide text-secondary">{u.role}</p>
                </div>
              </div>
            ))}

            {onlineUsers.length === 0 && (
              <div className="rounded-2xl border border-dashed border-line p-4 text-sm font-semibold text-secondary">
                Chua co thanh vien online.
              </div>
            )}
          </div>
        </div>
      </aside>

      <section className="grid min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)_auto] overflow-hidden">
        <header className="border-b border-line bg-white/95 px-4 py-3 backdrop-blur md:px-6">
          <div className="mx-auto flex max-w-5xl items-center justify-between gap-4">
            <div className="flex min-w-0 items-center gap-3">
              <div className="grid h-11 w-11 flex-none place-items-center rounded-2xl bg-slate-100 text-accent">
                <UsersThree size={24} weight="fill" />
              </div>
              <div className="min-w-0">
                <h2 className="truncate text-xl font-black md:text-2xl">General Chat</h2>
                <div className="mt-1 flex flex-wrap items-center gap-2 text-xs font-bold text-secondary">
                  <span className="inline-flex items-center gap-1.5">
                    {connectionReady ? (
                      <CheckCircle size={15} weight="fill" className="text-emerald-500" />
                    ) : (
                      <Circle size={13} weight="fill" className="text-amber-500" />
                    )}
                    {connectionReady ? 'Connected' : 'Connecting'}
                  </span>
                  <span>{memberCount} online</span>
                  {typingNames && <span className="text-accent">{typingNames} dang nhap...</span>}
                </div>
              </div>
            </div>

            <button onClick={logout} className="icon-button lg:hidden" title="Dang xuat" aria-label="Dang xuat">
              <SignOut size={21} weight="bold" />
            </button>
          </div>
        </header>

        <div ref={messagesContainerRef} className="min-h-0 overflow-y-auto px-3 py-5 md:px-6" aria-live="polite">
          {messages.length === 0 ? (
            <div className="mx-auto grid h-full max-w-md place-items-center text-center">
              <div>
                <div className="mx-auto mb-5 grid h-16 w-16 place-items-center rounded-3xl bg-white text-accent shadow-xl shadow-slate-200">
                  <ChatCircleDots size={32} weight="fill" />
                </div>
                <h3 className="text-2xl font-black">Phong chat dang trong</h3>
                <p className="mt-2 text-sm leading-6 text-secondary">
                  Gui tin nhan dau tien, dinh kem file hoac anh de bat dau test realtime.
                </p>
              </div>
            </div>
          ) : (
            <div className="mx-auto flex w-full max-w-5xl flex-col gap-2">
              {messages.map((m, index) => {
                const isMine = m.senderId === user.id;
                const isNextSameSender = index < messages.length - 1 && messages[index + 1].senderId === m.senderId;
                const isPrevSameSender = index > 0 && messages[index - 1].senderId === m.senderId;
                const state = (m.state ?? '').toLowerCase();
                const isActive = state === 'active';
                const wasRemoved = !isActive;
                const removedLabel = state === 'deletedbyadmin'
                  ? 'Tin nhan da bi admin xoa'
                  : 'Tin nhan da duoc thu hoi';

                return (
                  <article
                    key={m.id}
                    className={`group flex max-w-[88%] gap-2 md:max-w-[68%] ${isMine ? 'self-end flex-row-reverse' : 'self-start'} ${!isNextSameSender ? 'mb-3' : ''}`}
                  >
                    {!isMine && (
                      <div className="flex w-9 flex-none items-end pb-5">
                        {!isNextSameSender && (
                          <div className="grid h-9 w-9 place-items-center rounded-full bg-white font-black text-accent shadow-sm">
                            {initials(m.senderName)}
                          </div>
                        )}
                      </div>
                    )}

                    <div className={`min-w-0 ${isMine ? 'items-end' : 'items-start'} flex flex-col`}>
                      {!isMine && !isPrevSameSender && (
                        <span className="mb-1 ml-1 text-xs font-black text-secondary">{m.senderName}</span>
                      )}

                      <div
                        className={`min-w-0 rounded-2xl border px-4 py-3 shadow-sm ${
                          isMine
                            ? 'border-[#0877f2] bg-[#0877f2] text-white shadow-blue-100'
                            : 'border-line bg-white text-primary shadow-slate-200/70'
                        } ${wasRemoved ? '!border-dashed !border-slate-300 !bg-white !text-secondary' : ''}`}
                      >
                        {wasRemoved ? (
                          <div className="flex items-center gap-2 text-sm font-bold italic">
                            <ArrowCounterClockwise size={17} />
                            {removedLabel}
                          </div>
                        ) : (
                          <>
                            {m.text && <p className="whitespace-pre-wrap break-words text-[15px] leading-6">{m.text}</p>}

                            {m.attachment && (
                              <AttachmentPreview
                                attachment={m.attachment}
                                isMine={isMine}
                                onImageOpen={setFullscreenImage}
                              />
                            )}
                          </>
                        )}
                      </div>

                      <div className={`mt-1 flex items-center gap-2 text-[11px] font-bold text-slate-500 ${isMine ? 'flex-row-reverse' : ''}`}>
                        <span>{formatTime(m.createdAt)}</span>
                        {m.canRecall && (
                          <button onClick={() => handleRecall(m.id)} className="inline-flex items-center gap-1 rounded-md px-1 py-0.5 opacity-80 transition hover:bg-rose-50 hover:text-rose-600 focus:outline-none focus:ring-2 focus:ring-rose-200">
                            <ArrowCounterClockwise size={13} weight="bold" />
                            Thu hoi
                          </button>
                        )}
                        {m.canAdminDelete && (
                          <button onClick={() => handleAdminDelete(m.id)} className="inline-flex items-center gap-1 rounded-md px-1 py-0.5 opacity-80 transition hover:bg-rose-50 hover:text-rose-600 focus:outline-none focus:ring-2 focus:ring-rose-200">
                            <Trash size={13} weight="bold" />
                            Xoa
                          </button>
                        )}
                      </div>
                    </div>
                  </article>
                );
              })}
              {typingNames && <TypingIndicator names={typingNames} />}
              <div ref={messagesEndRef} />
            </div>
          )}
        </div>

        <footer className="border-t border-line bg-white px-3 py-3 md:px-6">
          <div className="mx-auto w-full max-w-5xl">
          {error && (
            <div className="mb-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-600" role="alert">
              {error}
            </div>
          )}

          {(previewUrl || selectedAttachment) && (
            <div className="mb-3 flex items-center justify-between gap-3 rounded-2xl border border-line bg-slate-50 p-3">
              <div className="flex min-w-0 items-center gap-3">
                {previewUrl ? (
                  <img src={previewUrl} alt="Preview" className="h-14 w-14 rounded-xl object-cover" />
                ) : (
                  <div className="grid h-12 w-12 place-items-center rounded-xl bg-white text-accent">
                    <File size={24} weight="fill" />
                  </div>
                )}
                <div className="min-w-0">
                  <p className="truncate text-sm font-black">{selectedAttachment?.fileName || 'Dang upload file'}</p>
                  <p className="text-xs font-semibold text-secondary">
                    {selectedAttachment ? fileSize(selectedAttachment.size) : 'Preview local'}
                  </p>
                </div>
              </div>
              <button onClick={clearAttachment} className="icon-button h-9 w-9" title="Bo dinh kem" aria-label="Bo dinh kem">
                <X size={18} weight="bold" />
              </button>
            </div>
          )}

          <div className="mb-2 flex items-center gap-2 overflow-x-auto" aria-label="Emoji nhanh">
            {emojiShortcuts.map(emoji => (
              <button
                key={emoji}
                onClick={() => setComposerText(prev => prev + emoji)}
                className="grid h-9 w-9 flex-none cursor-pointer place-items-center rounded-xl bg-slate-100 text-lg transition-colors duration-200 hover:bg-blue-50 focus:outline-none focus:ring-2 focus:ring-blue-200"
                title="Them emoji"
                aria-label={`Them emoji ${emoji}`}
              >
                {emoji}
              </button>
            ))}
          </div>

          <div className="flex items-end gap-2">
            <label className="icon-button h-11 w-11 flex-none cursor-pointer text-accent" title="Dinh kem file" aria-label="Dinh kem file">
              <input ref={fileInputRef} type="file" onChange={handleFileUpload} className="sr-only" aria-label="Dinh kem file" />
              <ImageSquare size={23} weight="bold" />
            </label>

            <label htmlFor="chat-composer" className="sr-only">Nhap tin nhan</label>
            <textarea
              id="chat-composer"
              value={composerText}
              onChange={handleTyping}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                  e.preventDefault();
                  handleSend();
                }
              }}
              placeholder="Nhap tin nhan..."
              className="min-h-11 max-h-32 flex-1 resize-none rounded-2xl border border-line bg-slate-50 px-4 py-3 text-[15px] leading-5 outline-none transition placeholder:text-slate-500 focus:border-accent focus:bg-white focus:ring-4 focus:ring-blue-100"
              rows="1"
            />

            {(composerText.trim() || selectedAttachment) ? (
              <button
                onClick={handleSend}
                disabled={sending}
                className="grid h-11 w-11 flex-none cursor-pointer place-items-center rounded-2xl bg-accent text-white shadow-lg shadow-blue-200 transition-colors duration-200 hover:bg-accentHover focus:outline-none focus:ring-4 focus:ring-blue-100 disabled:cursor-not-allowed disabled:opacity-60"
                title="Gui"
                aria-label="Gui tin nhan"
              >
                {sending ? <SpinnerGap size={22} weight="bold" className="spinner" /> : <PaperPlaneRight size={22} weight="fill" />}
              </button>
            ) : (
              <button
                onClick={sendQuickLike}
                disabled={sending || !connectionReady}
                className="grid h-11 w-11 flex-none cursor-pointer place-items-center rounded-2xl bg-blue-50 text-accent transition-colors duration-200 hover:bg-blue-100 focus:outline-none focus:ring-4 focus:ring-blue-100 disabled:cursor-not-allowed disabled:opacity-60"
                title="Gui like"
                aria-label="Gui like"
              >
                {sending ? <SpinnerGap size={21} weight="bold" className="spinner" /> : <ThumbsUp size={22} weight="fill" />}
              </button>
            )}
          </div>
          </div>
        </footer>
      </section>

      {fullscreenImage && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/95 p-4" onClick={() => setFullscreenImage(null)}>
          <button className="absolute right-4 top-4 grid h-11 w-11 cursor-pointer place-items-center rounded-2xl bg-white/10 text-white transition-colors duration-200 hover:bg-white/20 focus:outline-none focus:ring-4 focus:ring-white/30" onClick={() => setFullscreenImage(null)} aria-label="Dong anh">
            <X size={24} weight="bold" />
          </button>
          <img src={fullscreenImage} alt="Fullscreen" className="max-h-[92dvh] max-w-[94vw] rounded-2xl object-contain" onClick={e => e.stopPropagation()} />
        </div>
      )}
    </main>
  );
}

function TypingIndicator({ names }) {
  return (
    <div className="mb-3 flex max-w-[88%] items-end gap-2 self-start md:max-w-[68%]">
      <div className="grid h-9 w-9 flex-none place-items-center rounded-full bg-white text-xs font-black text-accent shadow-sm">
        ...
      </div>
      <div>
        <span className="mb-1 ml-1 block text-xs font-black text-secondary">{names}</span>
        <div className="inline-flex items-center gap-1 rounded-2xl border border-line bg-white px-4 py-3 shadow-sm shadow-slate-200/70">
          <span className="typing-dot" />
          <span className="typing-dot typing-dot-delay-1" />
          <span className="typing-dot typing-dot-delay-2" />
        </div>
      </div>
    </div>
  );
}

function AttachmentPreview({ attachment, isMine, onImageOpen }) {
  const url = `${getApiUrl()}${attachment.url}`;
  const kind = attachment.kind?.toLowerCase();

  if (kind === 'image') {
    return (
      <button type="button" className="mt-3 block overflow-hidden rounded-xl" onClick={() => onImageOpen(url)}>
        <img src={url} alt={attachment.fileName} className="max-h-[320px] w-full max-w-sm object-cover transition hover:opacity-95" />
      </button>
    );
  }

  if (kind === 'video') {
    return (
      <div className="mt-3 overflow-hidden rounded-xl bg-black">
        <video src={url} controls className="max-h-[320px] w-full max-w-sm" />
      </div>
    );
  }

  return (
    <a
      href={url}
      target="_blank"
      rel="noreferrer"
      className={`mt-3 flex items-center gap-3 rounded-xl border p-3 no-underline ${
        isMine ? 'border-white/25 bg-white/15 text-white' : 'border-line bg-slate-50 text-primary'
      }`}
    >
      <div className={`grid h-11 w-11 flex-none place-items-center rounded-xl ${isMine ? 'bg-white/20' : 'bg-white text-accent'}`}>
        {kind === 'video' ? <VideoCamera size={23} weight="fill" /> : <File size={23} weight="fill" />}
      </div>
      <div className="min-w-0">
        <p className="truncate text-sm font-black">{attachment.fileName}</p>
        <p className={`text-xs font-semibold ${isMine ? 'text-white/75' : 'text-secondary'}`}>
          {fileSize(attachment.size)}
        </p>
      </div>
    </a>
  );
}
