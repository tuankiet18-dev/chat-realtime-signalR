/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useState, useEffect } from 'react';
import { ChatCircleDots, SpinnerGap } from '@phosphor-icons/react';
import { apiCall } from '../services/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkAuth = async () => {
      const token = localStorage.getItem('chat_token');
      if (token) {
        try {
          const profile = await apiCall('/auth/me');
          setUser(profile);
        } catch {
          localStorage.removeItem('chat_token');
          setUser(null);
        }
      }
      setLoading(false);
    };

    checkAuth();
  }, []);

  const login = async (email, password) => {
    const data = await apiCall('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    localStorage.setItem('chat_token', data.token);
    const profile = await apiCall('/auth/me');
    setUser(profile);
  };

  const register = async (displayName, email, password) => {
    const data = await apiCall('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ displayName, email, password }),
    });
    localStorage.setItem('chat_token', data.token);
    const profile = await apiCall('/auth/me');
    setUser(profile);
  };

  const logout = () => {
    localStorage.removeItem('chat_token');
    setUser(null);
  };

  if (loading) {
    return (
      <main className="grid h-screen min-h-screen w-screen place-items-center bg-canvas px-4">
        <section className="w-full max-w-sm rounded-[28px] border border-line bg-white p-8 text-center shadow-2xl shadow-slate-200/70">
          <div className="mx-auto mb-5 grid h-14 w-14 place-items-center rounded-2xl bg-accent text-white shadow-lg shadow-blue-200">
            <ChatCircleDots size={30} weight="fill" />
          </div>
          <h1 className="text-2xl font-black text-primary">ChatApp</h1>
          <p className="mt-2 text-sm font-semibold text-secondary">Dang kiem tra phien dang nhap...</p>
          <SpinnerGap size={26} weight="bold" className="spinner mx-auto mt-6 text-accent" />
        </section>
      </main>
    );
  }

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
