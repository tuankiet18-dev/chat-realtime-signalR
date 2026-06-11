import { useState } from 'react';
import { IdentificationBadge, Key, UserPlus } from '@phosphor-icons/react';
import AuthLayout from './AuthLayout';
import { useAuth } from '../contexts/AuthContext';

export default function Register({ onSwitchToLogin }) {
  const { register } = useAuth();
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register(displayName, email, password);
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      eyebrow="Create account"
      title="Tao tai khoan"
      description="Dang ky user moi de test chat realtime. Admin seed van giu trong backend de test quyen xoa tin nhan."
    >
      <form onSubmit={handleSubmit} className="grid gap-5">
        <label htmlFor="register-display-name" className="grid gap-2 text-sm font-black text-primary">
          Ten hien thi
          <input
            id="register-display-name"
            type="text"
            placeholder="Vi du: Kiet"
            value={displayName}
            onChange={e => setDisplayName(e.target.value)}
            className="input-field"
            autoComplete="name"
            required
          />
        </label>

        <label htmlFor="register-email" className="grid gap-2 text-sm font-black text-primary">
          Email
          <input
            id="register-email"
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={e => setEmail(e.target.value)}
            className="input-field"
            autoComplete="email"
            required
          />
        </label>

        <label htmlFor="register-password" className="grid gap-2 text-sm font-black text-primary">
          Password
          <input
            id="register-password"
            type="password"
            placeholder="Nhap mat khau"
            value={password}
            onChange={e => setPassword(e.target.value)}
            className="input-field"
            autoComplete="new-password"
            required
          />
        </label>

        <button type="submit" disabled={loading} className="btn-primary mt-1 gap-2">
          {loading ? <IdentificationBadge size={20} weight="fill" /> : <UserPlus size={20} weight="fill" />}
          {loading ? 'Dang tao...' : 'Tao tai khoan'}
        </button>
      </form>

      {error && (
        <p className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700" role="alert">
          {error}
        </p>
      )}

      <div className="mt-6 rounded-2xl border border-line bg-slate-50 p-4">
        <div className="mb-2 flex items-center gap-2 text-sm font-black text-primary">
          <Key size={19} weight="fill" className="text-rose-500" />
          Role mac dinh
        </div>
        <p className="text-sm font-semibold leading-6 text-secondary">
          Tai khoan dang ky tu UI se la user. Dung admin seed de test xoa tin nhan cua nguoi khac.
        </p>
      </div>

      <div className="mt-6 flex items-center justify-between gap-3 text-sm">
        <span className="font-semibold text-secondary">Da co tai khoan?</span>
        <button type="button" onClick={onSwitchToLogin} className="font-black text-accent transition-colors hover:text-accentHover focus:outline-none focus:ring-4 focus:ring-blue-100">
          Dang nhap
        </button>
      </div>
    </AuthLayout>
  );
}
