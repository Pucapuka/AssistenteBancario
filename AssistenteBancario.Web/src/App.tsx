import { useCallback, useEffect, useState } from "react";

import Sidebar from "./components/Sidebar/Sidebar";
import Header from "./components/Header/Header";
import Dashboard from "./components/Dashboard/Dashboard";
import Operations from "./components/Operations/Operations";

import { consultarSaldo } from "./services/bancoService";

import "./App.css";

function App() {
  const [saldo, setSaldo] = useState<number | null>(null);
  const [carregandoSaldo, setCarregandoSaldo] = useState(true);
  const [erroSaldo, setErroSaldo] = useState<string | null>(null);

  const [mostrarOperacoes, setMostrarOperacoes] = useState(false);
  const [numeroContaAtiva, setNumeroContaAtiva] = useState(1001);

  // Consulta o saldo da conta atualmente selecionada.
  // Também pode ser chamada pelo Dashboard após uma operação do assistente.
  const atualizarSaldo = useCallback(async () => {
    try {
      setCarregandoSaldo(true);
      setErroSaldo(null);

      const resultado = await consultarSaldo(numeroContaAtiva);

      if (!resultado.sucesso || resultado.saldoAtual === null) {
        throw new Error(resultado.mensagem);
      }

      setSaldo(resultado.saldoAtual);
    } catch (error) {
      console.error("Erro ao carregar saldo:", error);
      setErroSaldo("Não foi possível carregar o saldo.");
    } finally {
      setCarregandoSaldo(false);
    }
  }, [numeroContaAtiva]);

  useEffect(() => {
    void atualizarSaldo();
  }, [atualizarSaldo]);

  function handleOperacaoManualConcluida(novoSaldo: number) {
    setSaldo(novoSaldo);
    setErroSaldo(null);
  }

  return (
    <div className="app-container">
      <Sidebar />

      <main className="main-content" id="dashboard">
        <Header />

        <Dashboard
          saldo={saldo}
          carregandoSaldo={carregandoSaldo}
          erroSaldo={erroSaldo}
          numeroConta={numeroContaAtiva}
          onSelectAccount={setNumeroContaAtiva}
          onOpenOperations={() => setMostrarOperacoes(true)}
          onRefreshSaldo={atualizarSaldo}
        />
      </main>

      {mostrarOperacoes && (
        <Operations
          numeroConta={numeroContaAtiva}
          onClose={() => setMostrarOperacoes(false)}
          onSuccess={handleOperacaoManualConcluida}
        />
      )}
    </div>
  );
}

export default App;