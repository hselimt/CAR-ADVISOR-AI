import React, { useState } from 'react';
import axios from 'axios';
import './App.css';

function App() {
    const [minBudget, setMinBudget] = useState('');
    const [maxBudget, setMaxBudget] = useState('');
    const [preferences, setPreferences] = useState('');
    const [country, setCountry] = useState('Turkey');
    const [currency, setCurrency] = useState('TRY');
    const [started, setStarted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [agents, setAgents] = useState([]);
    const [winner, setWinner] = useState(null);
    const [selectedAgent, setSelectedAgent] = useState(null);
    const [processingStage, setProcessingStage] = useState(null);

    const countries = [
        { name: 'Turkey', currency: 'TRY', symbol: '₺' },
        { name: 'USA', currency: 'USD', symbol: '$' },
        { name: 'Germany', currency: 'EUR', symbol: '€' },
        { name: 'UK', currency: 'GBP', symbol: '£' },
        { name: 'Japan', currency: 'JPY', symbol: '¥' },
        { name: 'UAE', currency: 'AED', symbol: 'د.إ' }
    ];

    const agentIcons = {
        'Ultra-Luxury': '💎', 'Premium': '⭐', 'Upper-Mainstream': '🎯',
        'Mainstream': '🔧', 'Budget': '💸', 'Electric': '⚡'
    };

    const allAgentNames = ['Ultra-Luxury', 'Premium', 'Upper-Mainstream', 'Mainstream', 'Budget', 'Electric'];

    const handleQuit = () => {
        if (window.electronAPI) window.electronAPI.quitApp();
    };

    const formatBudget = (value) => {
        const numbers = value.replace(/\D/g, '');
        return numbers.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const cleanMin = minBudget.replace(/[^\d]/g, '');
        const cleanMax = maxBudget.replace(/[^\d]/g, '');

        if (!cleanMin || !cleanMax || !preferences) return alert('Please fill all fields');

        setStarted(true);
        setLoading(true);
        setAgents([]);
        setWinner(null);
        setProcessingStage('agents');

        try {
            // Backend call
            const response = await axios.post('http://localhost:5163/api/CarAdvisor/analyze', {
                minBudget: cleanMin,
                maxBudget: cleanMax,
                preferences: preferences.trim(),
                country,
                currency
            });

            setAgents(response.data.recommendations);
            setProcessingStage('agents-complete');

            // Simulate delay
            setTimeout(() => {
                setProcessingStage('jury');
                setTimeout(() => {
                    setWinner(response.data.winner);
                    setLoading(false);
                    setProcessingStage('complete');
                }, 1500);
            }, 800);

        } catch (error) {
            console.error(error);
            alert('Error: ' + (error.response?.data?.error || error.message));
            setLoading(false);
            setStarted(false);
        }
    };

    const handleReset = () => {
        setStarted(false);
        setLoading(false);
        setAgents([]);
        setWinner(null);
        setMinBudget('');
        setMaxBudget('');
        setPreferences('');
        setSelectedAgent(null);
    };

    // Helper to generate small text for agent cards
    const getAgentSummary = (agent) => {
        if (!agent || !agent.suggestions || agent.suggestions.length === 0) {
            if (agent?.budgetStatus === 'too-expensive') return 'ABOVE BUDGET';
            if (agent?.budgetStatus === 'too-cheap') return 'BELOW BUDGET';
            return 'NO MATCHES';
        }
        // Return structured names instead of parsing markdown
        return agent.suggestions.map(c => `${c.make} ${c.model}`).join(' • ').toUpperCase();
    };

    return (
        <div className="App">
            {window.electronAPI && <button className="quit-button" onClick={handleQuit}>×</button>}
            <header className="App-header">
                <div className="header-content">
                    <h1>AI CAR ADVISOR</h1>
                    <p>Multi-Agent System • Fintech Architecture</p>
                </div>
            </header>

            <main className="App-main">
                {!started ? (
                    <div className="input-form">
                        <h2>YOUR REQUIREMENTS</h2>
                        <form onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label>MARKET</label>
                                <select value={country} onChange={e => {
                                    const c = countries.find(x => x.name === e.target.value);
                                    setCountry(c.name); setCurrency(c.currency);
                                }} className="form-select">
                                    {countries.map(c => <option key={c.name} value={c.name}>{c.name}</option>)}
                                </select>
                            </div>
                            <div className="form-group">
                                <label>BUDGET ({countries.find(c => c.name === country).symbol})</label>
                                <div className="budget-range">
                                    <input value={minBudget} onChange={e => setMinBudget(formatBudget(e.target.value))} className="budget-input" placeholder="Min" />
                                    <span className="budget-separator">-</span>
                                    <input value={maxBudget} onChange={e => setMaxBudget(formatBudget(e.target.value))} className="budget-input" placeholder="Max" />
                                </div>
                            </div>
                            <div className="form-group">
                                <label>PREFERENCES</label>
                                <textarea value={preferences} onChange={e => setPreferences(e.target.value)} className="form-textarea" rows="3" />
                            </div>
                            <button type="submit">START ANALYSIS</button>
                        </form>
                    </div>
                ) : (
                    <div className="results">
                        {/* Progress Bar Visualization */}
                        <div className="progress-tracker">
                            <div className="progress-header">
                                <span className="progress-title">
                                    {processingStage === 'agents' && 'AGENTS ANALYZING...'}
                                    {processingStage === 'jury' && 'JURY DELIBERATION...'}
                                    {processingStage === 'complete' && 'VERDICT READY'}
                                </span>
                            </div>
                        </div>

                        {/* Agent Cards */}
                        {(!loading || agents.length > 0) && (
                            <div className="agents-grid">
                                {allAgentNames.map((name, index) => {
                                    const agent = agents.find(a => a.agentName === name);
                                    const summary = getAgentSummary(agent);
                                    const isDimmed = summary === 'ABOVE BUDGET' || summary === 'BELOW BUDGET' || summary === 'NO MATCHES';

                                    return (
                                        <div key={index}
                                            className={`agent-card-compact ${isDimmed ? 'out-of-range' : ''}`}
                                            onClick={() => agent && agent.suggestions.length > 0 && setSelectedAgent(agent)}>
                                            <div className="agent-header-compact">
                                                <div className="agent-icon-compact">{agentIcons[name]}</div>
                                                <div className="agent-name-compact">{name}</div>
                                            </div>
                                            <div className={`agent-cars-compact ${isDimmed ? 'out-of-range-text' : ''}`}>
                                                {loading && !agent ? '...' : summary}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}

                        {/* Winner Card */}
                        {winner && (
                            <div className="winner-card-compact">
                                <div className="winner-header-compact">
                                    <span className="winner-icon">🏆</span>
                                    <span className="winner-title-compact">FINAL VERDICT</span>
                                </div>
                                <div className="winner-preview-compact" style={{ fontSize: '1.2rem', color: 'white' }}>
                                    {winner.winningCar}
                                </div>
                                <div style={{ textAlign: 'center', color: '#4a9eff', fontSize: '0.9rem', marginTop: '5px' }}>
                                    Score: {winner.totalScore}/100
                                </div>
                                <button className="expand-button-compact" onClick={() => setSelectedAgent({ isJury: true, ...winner })}>
                                    VIEW REPORT
                                </button>
                            </div>
                        )}

                        {!loading && <button className="reset-button" onClick={handleReset}>NEW SEARCH</button>}
                    </div>
                )}
            </main>

            {/* Detail Modal - Now uses CLEAN DATA, no dangerous HTML */}
            {selectedAgent && (
                <div className="modal-overlay" onClick={() => setSelectedAgent(null)}>
                    <div className="modal-content" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <h3>{selectedAgent.isJury ? 'Jury Verdict' : selectedAgent.agentName}</h3>
                            <button className="modal-close" onClick={() => setSelectedAgent(null)}>×</button>
                        </div>
                        <div className="modal-body">
                            {selectedAgent.isJury ? (
                                <div>
                                    <h2 style={{ color: '#4a9eff' }}>{selectedAgent.winningCar}</h2>
                                    <p>{selectedAgent.finalVerdict}</p>
                                    <div style={{ marginTop: '15px', borderTop: '1px solid #333', paddingTop: '10px' }}>
                                        {Object.entries(selectedAgent.detailedScores).map(([key, val]) => (
                                            <div key={key} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '5px' }}>
                                                <span>{key}</span>
                                                <span style={{ color: '#10b981', fontWeight: 'bold' }}>{val}</span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            ) : (
                                <div>
                                    {selectedAgent.suggestions.map((car, i) => (
                                        <div key={i} style={{ marginBottom: '20px', borderBottom: '1px solid #333', paddingBottom: '10px' }}>
                                            <h4 style={{ color: '#e0e0e0', fontSize: '1.1rem' }}>{car.make} {car.model} ({car.year})</h4>
                                            <div style={{ color: '#4a9eff', fontWeight: 'bold' }}>{car.price} {currency}</div>
                                            <ul style={{ color: '#909090', fontSize: '0.85rem', paddingLeft: '20px', marginTop: '5px' }}>
                                                <li>Engine: {car.engine}</li>
                                                <li>Fuel: {car.fuel}</li>
                                                <li>Safety: {car.safetyRating}</li>
                                            </ul>
                                            <p style={{ fontStyle: 'italic', color: '#b0b0b0', marginTop: '8px' }}>"{car.reasoning}"</p>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default App;