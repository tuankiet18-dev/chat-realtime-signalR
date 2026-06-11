import { useState } from 'react';
import { ArrowRight, LockKey } from '@phosphor-icons/react';
import AuthLayout from './AuthLayout';
import { useAuth } from '../contexts/AuthContext';

export default function Login({ onSwitchToRegister }) {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Dang nhap"
    >
      <form onSubmit={handleSubmit} className="grid gap-5">
        <label htmlFor="login-email" className="grid gap-2 text-sm font-black text-primary">
          Email
          <input
            id="login-email"
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={e => setEmail(e.target.value)}
            className="input-field"
            autoComplete="email"
            required
          />
        </label>

        <label htmlFor="login-password" className="grid gap-2 text-sm font-black text-primary">
          Password
          <input
            id="login-password"
            type="password"
            placeholder="Nhap mat khau"
            value={password}
            onChange={e => setPassword(e.target.value)}
            className="input-field"
            autoComplete="current-password"
            required
          />
        </label>

        <button type="submit" disabled={loading} className="btn-primary mt-1 gap-2">
          <LockKey size={20} weight="fill" />
          {loading ? 'Dang dang nhap...' : 'Dang nhap'}
          {!loading && <ArrowRight size={18} weight="bold" />}
        </button>
      </form>

      {error && (
        <p className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700" role="alert">
          {error}
        </p>
      )}

      <div className="mt-6 flex items-center justify-between gap-3 text-sm">
        <span className="font-semibold text-secondary">Chua co tai khoan?</span>
        <button type="button" onClick={onSwitchToRegister} className="font-black text-accent transition-colors hover:text-accentHover focus:outline-none focus:ring-4 focus:ring-blue-100">
          Tao tai khoan
        </button>
      </div>
    </AuthLayout>
  );
}
