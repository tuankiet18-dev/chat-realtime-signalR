import { ChatCircleDots } from '@phosphor-icons/react';

export default function AuthLayout({ title, description, children }) {
  return (
    <main className="relative grid min-h-screen place-items-center overflow-hidden bg-[#f8fbff] px-4 py-8 antialiased">
      {/* Soft background blobs for modern look */}
      <div className="pointer-events-none absolute -left-[10%] -top-[10%] h-[50vw] w-[50vw] rounded-full bg-blue-400/10 blur-[100px]" />
      <div className="pointer-events-none absolute -bottom-[10%] -right-[10%] h-[50vw] w-[50vw] rounded-full bg-purple-400/10 blur-[100px]" />

      <section className="relative z-10 w-full max-w-[420px] rounded-[28px] border border-white/60 bg-white/80 p-8 shadow-[0_8px_40px_-12px_rgba(0,0,0,0.1)] backdrop-blur-xl sm:p-10">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="mb-5 grid h-14 w-14 place-items-center rounded-2xl bg-gradient-to-br from-[#0084ff] to-[#005bb5] text-white shadow-lg shadow-blue-500/30">
            <ChatCircleDots size={32} weight="fill" />
          </div>
          <h1 className="mb-2 text-2xl font-extrabold tracking-tight text-gray-900">{title}</h1>
          {description && (
            <p className="text-[15px] font-medium leading-relaxed text-gray-500">{description}</p>
          )}
        </div>
        
        {children}
      </section>
    </main>
  );
}
