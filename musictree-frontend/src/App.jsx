import { BrowserRouter, Routes, Route, Outlet } from "react-router-dom";
import 'bootstrap/dist/css/bootstrap.min.css';
import Nav from "./Components/Nav";

// Menu
import MenuLogin from "./VistaMenu/MenuLogin";

// Curador 
import LoginCurador from "./VistaCurador/LoginCurador";
import RegistroCurador from "./VistaCurador/RegistroCurador";
import CrearCluster from "./VistaCurador/CrearCluster";
import CrearGenero from "./VistaCurador/CrearGenero";
import VerCluster from "./VistaCurador/VerCluster";
import ImportarGeneros from "./VistaCurador/ImportarGeneros";
import CatalogoArtistas from "./VistaCurador/CatalogoArtistas";
import CrearArtista from "./VistaCurador/CrearArtista";
import MenuCurador from "./VistaCurador/MenuCurador";

// Fanático
import LoginFanaticos from "./VistaFanaticos/LoginFanaticos";
import RegistroFanaticos from "./VistaFanaticos/RegistroFanaticos";
import BuscarArtistaFanaticos from "./VistaFanaticos/BuscarArtistaFanaticos";
import MenuFanaticos from "./VistaFanaticos/MenuFanaticos";
import PerfilArtista from "./VistaFanaticos/PerfilArtista";

// Layout general con Nav
const LayoutConNav = () => (
  <>
    <Nav />
    <Outlet />
  </>
);

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Logins y registros */}
        <Route path="/" element={<MenuLogin />} />
        <Route path="/logincurador" element={<LoginCurador />} />
        <Route path="/registercurador" element={<RegistroCurador />} />
        <Route path="/loginfanaticos" element={<LoginFanaticos />} />
        <Route path="/registerfanaticos" element={<RegistroFanaticos />} />

        {/* Rutas protegidas bajo el layout con navbar */}
        <Route element={<LayoutConNav />}>
          {/* Curador */}
          <Route path="/curador/menucurador" element={<MenuCurador />} />
          <Route path="/curador/crearcluster" element={<CrearCluster />} />
          <Route path="/curador/vercluster" element={<VerCluster />} />
          <Route path="/curador/creargenero" element={<CrearGenero />} />
          <Route path="/curador/importargeneros" element={<ImportarGeneros />} />
          <Route path="/curador/catalogoartistas" element={<CatalogoArtistas />} />
          <Route path="/curador/crearartista" element={<CrearArtista />} />

          {/* Fanático */}
          <Route path="/fanaticos/menufanaticos" element={<MenuFanaticos />} />
          <Route path="/fanaticos/buscarartistafanaticos" element={<BuscarArtistaFanaticos />} />
          <Route path="/fanaticos/perfilartista/:id" element={<PerfilArtista />} />

        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
