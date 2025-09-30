import React, { useState } from 'react';

function LoginForm({ onLogin }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const response = await fetch('/api/protected', {
        method: 'GET',
        headers: {
          'Authorization': 'Basic ' + btoa(username + ':' + password)
        }
      });
      if (!response.ok) throw new Error('Authentication failed');
      const data = await response.json();
      onLogin(username);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Login</h2>
      <label>Username:</label><br />
      <input type="text" value={username} onChange={e => setUsername(e.target.value)} /><br />
      <label>Password:</label><br />
      <input type="password" value={password} onChange={e => setPassword(e.target.value)} /><br />
      <button type="submit" disabled={loading}>Login</button>
      {error && <div style={{color:'red'}}>{error}</div>}
    </form>
  );
}

export default LoginForm;
