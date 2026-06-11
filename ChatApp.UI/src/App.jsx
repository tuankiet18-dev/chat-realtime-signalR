import { useState } from 'react';
import { useAuth } from './contexts/AuthContext';
import Login from './components/Login';
import Register from './components/Register';
import Chat from './components/Chat';

function App() {
  const { user } = useAuth();
  const [showLogin, setShowLogin] = useState(true);

  if (!user) {
    return showLogin ? (
      <Login onSwitchToRegister={() => setShowLogin(false)} />
    ) : (
      <Register onSwitchToLogin={() => setShowLogin(true)} />
    );
  }

  return <Chat />;
}

export default App;
