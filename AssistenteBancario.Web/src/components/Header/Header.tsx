import { Bell, Menu, Search } from "lucide-react";

function Header() {
  return (
    <header className="topbar">
      <div className="page-title">
        <button className="mobile-menu" aria-label="Abrir menu">
          <Menu size={21} />
        </button>

        <div>
          <span className="breadcrumb">Visão geral /</span>
          <h1>Dashboard</h1>
        </div>
      </div>

      <div className="topbar-actions">
        <button className="icon-button" aria-label="Pesquisar">
          <Search size={19} />
        </button>

        <button
          className="icon-button notification"
          aria-label="Notificações"
        >
          <Bell size={19} />
          <span />
        </button>

        <div className="topbar-avatar">PA</div>
      </div>
    </header>
  );
}

export default Header;