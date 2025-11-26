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
    const [winnerImage, setWinnerImage] = useState(null);
    const [imageLoading, setImageLoading] = useState(false);

    const countries = [
        { name: 'Turkey', currency: 'TRY', symbol: '₺' },
        { name: 'USA', currency: 'USD', symbol: '$' },
        { name: 'Germany', currency: 'EUR', symbol: '€' },
        { name: 'UK', currency: 'GBP', symbol: '£' },
        { name: 'Japan', currency: 'JPY', symbol: '¥' },
        { name: 'UAE', currency: 'AED', symbol: 'د.إ' }
    ];

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
            const response = await axios.post('http://localhost:5163/api/CarAdvisor/analyze', {
                minBudget: cleanMin,
                maxBudget: cleanMax,
                preferences: preferences.trim(),
                country,
                currency
            });

            setAgents(response.data.recommendations);
            setProcessingStage('jury');

            setTimeout(() => {
                setWinner(response.data.winner);
                setLoading(false);
                setProcessingStage('complete');
            }, 1000);

        } catch (error) {
            console.error(error);
            alert('Error: ' + (error.response?.data?.error || error.message));
            setLoading(false);
            setStarted(false);
            setProcessingStage(null);
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
        setProcessingStage(null);
        setWinnerImage(null);
        setImageLoading(false);
    };

    const getAgentData = (agentName) => {
        const agent = agents.find(a => a.agentName === agentName);
        
        if (!agent) {
            return { text: 'NOT INCLUDED', status: 'not-included' };
        }

        const budgetStatus = agent.budgetStatus || agent.BudgetStatus;
        
        if (budgetStatus === 'too-expensive') {
            return { text: 'ABOVE BUDGET', status: 'out-of-range' };
        }
        if (budgetStatus === 'too-cheap') {
            return { text: 'BELOW BUDGET', status: 'out-of-range' };
        }

        if (!agent.suggestions || agent.suggestions.length === 0) {
            return { text: 'NO SUGGESTIONS', status: 'out-of-range' };
        }

        const cars = agent.suggestions.map(c => `${c.make || c.Make} ${c.model || c.Model}`).join(' / ');
        return { 
            text: cars.length > 40 ? cars.substring(0, 40) + '...' : cars, 
            status: 'has-results',
            clickable: true,
            agent: agent
        };
    };

    const getCardState = () => {
        if (agents.length > 0) return 'done';
        if (processingStage === 'agents') return 'processing';
        return 'waiting';
    };

    const generateWinnerImage = async (carName) => {
        if (!carName || winnerImage) return;
        setImageLoading(true);
        try {
            const response = await axios.post('http://localhost:5163/api/CarAdvisor/generate-image', {
                carName: carName
            });
            setWinnerImage(response.data.image);
        } catch (error) {
            console.error('Image generation failed:', error);
        }
        setImageLoading(false);
    };

    // Auto-generate image when jury modal opens
    React.useEffect(() => {
        if (selectedAgent?.isJury && selectedAgent?.winningCar && !winnerImage && !imageLoading) {
            generateWinnerImage(selectedAgent.winningCar);
        }
    }, [selectedAgent]);

    return (
        <div className="app">
            {window.electronAPI && (
                <button className="quit-btn" onClick={handleQuit}>x</button>
            )}

            <header className="header">
                <h1>AI CAR ADVISOR</h1>
                <span className="subtitle">Multi-Agent Recommendation System</span>
            </header>

            <main className="main">
                {!started ? (
                    <div className="form-container">
                        <div className="form-title">REQUIREMENTS</div>
                        <form onSubmit={handleSubmit}>
                            <div className="field">
                                <label>MARKET</label>
                                <select value={country} onChange={e => {
                                    const c = countries.find(x => x.name === e.target.value);
                                    setCountry(c.name); setCurrency(c.currency);
                                }}>
                                    {countries.map(c => (
                                        <option key={c.name} value={c.name}>{c.name}</option>
                                    ))}
                                </select>
                            </div>

                            <div className="field">
                                <label>BUDGET ({countries.find(c => c.name === country).symbol})</label>
                                <div className="budget-row">
                                    <input 
                                        value={minBudget} 
                                        onChange={e => setMinBudget(formatBudget(e.target.value))} 
                                        placeholder="Min" 
                                    />
                                    <span className="separator">-</span>
                                    <input 
                                        value={maxBudget} 
                                        onChange={e => setMaxBudget(formatBudget(e.target.value))} 
                                        placeholder="Max" 
                                    />
                                </div>
                            </div>

                            <div className="field">
                                <label>PREFERENCES</label>
                                <textarea 
                                    value={preferences} 
                                    onChange={e => setPreferences(e.target.value)} 
                                    rows="2"
                                    placeholder="e.g., Fuel efficient, safe, modern tech..."
                                />
                            </div>

                            <button type="submit" className="submit-btn">ANALYZE</button>
                        </form>
                    </div>
                ) : (
                    <div className="results">
                        <div className="status-bar">
                            <span className="status-text">
                                {processingStage === 'agents' && 'ANALYZING...'}
                                {processingStage === 'jury' && 'JURY DECIDING...'}
                                {processingStage === 'complete' && 'COMPLETE'}
                            </span>
                        </div>

                        <div className="agents-grid">
                            {allAgentNames.map((agentName, index) => {
                                const cardState = getCardState();
                                const agentData = cardState === 'done' ? getAgentData(agentName) : null;

                                return (
                                    <div 
                                        key={index}
                                        className={`agent-card ${cardState} ${agentData?.status || ''}`}
                                        onClick={() => agentData?.clickable && setSelectedAgent(agentData.agent)}
                                    >
                                        <div className="agent-name">{agentName}</div>
                                        <div className="agent-result">
                                            {cardState === 'processing' && (
                                                <div className="spinner"></div>
                                            )}
                                            {cardState === 'done' && agentData && (
                                                <span className={`result-text ${agentData.status}`}>
                                                    {agentData.text}
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>

                        {(processingStage === 'jury' || processingStage === 'complete') && (
                            <div className={`jury-section ${processingStage === 'complete' ? 'done' : ''}`}>
                                <div className="jury-label">JURY</div>
                                {processingStage === 'jury' && (
                                    <div className="jury-loading">
                                        <div className="spinner"></div>
                                    </div>
                                )}
                                {processingStage === 'complete' && winner && (
                                    <div className="jury-result">
                                        <div className="winner-car">{winner.winningCar}</div>
                                        <div className="winner-price">{winner.winningCarPrice}</div>
                                        <div className="winner-score">Score: {winner.totalScore}/100</div>
                                        <button className="report-btn" onClick={() => setSelectedAgent({ isJury: true, ...winner })}>
                                            VIEW REPORT
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}

                        {!loading && (
                            <button className="reset-btn" onClick={handleReset}>NEW SEARCH</button>
                        )}
                    </div>
                )}
            </main>

            <footer className="footer">
                Powered by Gemini 1.5
            </footer>

            {selectedAgent && (
                <div className="modal-overlay" onClick={() => setSelectedAgent(null)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <span>{selectedAgent.isJury ? 'Jury Verdict' : selectedAgent.agentName}</span>
                            <button className="close-btn" onClick={() => setSelectedAgent(null)}>x</button>
                        </div>
                        <div className="modal-body">
                            {selectedAgent.isJury ? (
                                <div className="jury-report">
                                    <div className="verdict-car">{selectedAgent.winningCar}</div>
                                    <div className="verdict-price">{selectedAgent.winningCarPrice}</div>
                                    
                                    <div className="verdict-image">
                                        {imageLoading ? (
                                            <div className="image-loading">
                                                <div className="spinner"></div>
                                            </div>
                                        ) : winnerImage ? (
                                            <img src={winnerImage} alt={selectedAgent.winningCar} />
                                        ) : (
                                            <div className="image-placeholder">No image</div>
                                        )}
                                    </div>

                                    <p className="verdict-text">{selectedAgent.finalVerdict}</p>
                                    
                                    <div className="scores">
                                        {selectedAgent.detailedScores && Object.entries(selectedAgent.detailedScores).map(([key, val]) => (
                                            <div key={key} className="score-row">
                                                <span className="score-label">{key}</span>
                                                <div className="score-bar">
                                                    <div className="score-fill" style={{ width: `${(val / 20) * 100}%` }} />
                                                </div>
                                                <span className="score-val">{val}</span>
                                            </div>
                                        ))}
                                    </div>

                                    {selectedAgent.keyStrengths && (
                                        <div className="strengths">
                                            <div className="strengths-title">Key Strengths</div>
                                            {selectedAgent.keyStrengths.map((s, i) => (
                                                <div key={i} className="strength-item">- {s}</div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            ) : (
                                <div className="cars-list">
                                    {selectedAgent.suggestions?.map((car, i) => (
                                        <div key={i} className="car-item">
                                            <div className="car-name">
                                                {car.make || car.Make} {car.model || car.Model} 
                                                <span className="car-year">{car.year || car.Year}</span>
                                            </div>
                                            <div className="car-price">{car.price || car.Price}</div>
                                            <div className="car-specs">
                                                <span>Engine: {car.engine || car.Engine}</span>
                                                <span>Fuel: {car.fuel || car.Fuel}</span>
                                                <span>Safety: {car.safetyRating || car.SafetyRating}</span>
                                            </div>
                                            <div className="car-reasoning">{car.reasoning || car.Reasoning}</div>
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
