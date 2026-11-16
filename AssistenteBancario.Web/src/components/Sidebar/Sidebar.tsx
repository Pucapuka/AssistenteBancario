import {
    LayoutDashboard,
    ArrowLeftRight,
    MessageCircle,
    Settings,
    HelpCircle,
    Wallet,
  } from "lucide-react";
  
  function Sidebar() {
    return (
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">
            <Wallet size={23} />
          </div>
          <span>
            Moneta<span className="brand-highlight">AI</span>
          </span>
        </div>
  
        <div className="profile">
          <div className="avatar">PA</div>
          <div>
            <strong>Paulo Anderson</strong>
            <span>Conta pessoal</span>
          </div>
        </div>
  
        <span className="nav-label">MENU PRINCIPAL</span>
  
        <nav className="navigation">
          <a className="nav-item active" href="#dashboard">
            <LayoutDashboard size={19} />
            Dashboard
          </a>
  
          <a className="nav-item" href="#transactions">
            <ArrowLeftRight size={19} />
            Transações
          </a>
  
          <a className="nav-item" href="#assistant">
            <MessageCircle size={19} />
            Assistente IA
          </a>
        </nav>
  
        <div className="sidebar-bottom">
          <a className="nav-item" href="#help">
            <HelpCircle size={19} />
            Ajuda
          </a>
  
          <a className="nav-item" href="#settings">
            <Settings size={19} />
            Configurações
          </a>
  
          <div className="sidebar-footer">
            <span>Moneta AI</span>
            <span>Versão 0.1.0</span>
          </div>
        </div>
      </aside>
    );
  }
  
  export default Sidebar;