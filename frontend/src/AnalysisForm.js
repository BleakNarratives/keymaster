import React, { useState } from 'react';

function AnalysisForm({ calibrationResult }) {
  const [prompt, setPrompt] = useState('');
  const [analysis, setAnalysis] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  if (!calibrationResult) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const response = await fetch('/api/analyze', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ result: calibrationResult, prompt })
      });
      if (!response.ok) throw new Error('API error');
      const data = await response.json();
      setAnalysis(data.analysis);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>AI Analysis</h2>
      <form onSubmit={handleSubmit}>
        <label>Prompt:</label><br />
        <input type="text" value={prompt} onChange={e => setPrompt(e.target.value)} /><br />
        <button type="submit" disabled={loading}>Analyze</button>
      </form>
      {error && <div style={{color:'red'}}>{error}</div>}
      {analysis && <div style={{marginTop:'1em'}}><strong>Result:</strong> {analysis}</div>}
    </div>
  );
}

export default AnalysisForm;
