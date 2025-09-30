import React, { useState } from 'react';

function CalibrationUpload({ onResult }) {
  const [arwPoints, setArwPoints] = useState('');
  const [rwpPoints, setRwpPoints] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const arw = arwPoints.split('\n').map(line => {
        const [x, y, z] = line.split(',').map(Number);
        return { x, y, z };
      });
      const rwp = rwpPoints.split('\n').map(line => {
        const [x, y, z] = line.split(',').map(Number);
        return { x, y, z };
      });
      const response = await fetch('/api/calibrate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ arw_points: arw, rwp_points: rwp })
      });
      if (!response.ok) throw new Error('API error');
      const result = await response.json();
      onResult(result);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Upload Calibration Points</h2>
      <label>ARW Points (x,y,z per line):</label><br />
      <textarea value={arwPoints} onChange={e => setArwPoints(e.target.value)} rows={5} /><br />
      <label>RWP Points (x,y,z per line):</label><br />
      <textarea value={rwpPoints} onChange={e => setRwpPoints(e.target.value)} rows={5} /><br />
      <button type="submit" disabled={loading}>Calibrate</button>
      {error && <div style={{color:'red'}}>{error}</div>}
    </form>
  );
}

export default CalibrationUpload;
