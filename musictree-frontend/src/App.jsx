import { BrowserRouter, Routes, Route, Outlet } from "react-router-dom";
import 'bootstrap/dist/css/bootstrap.min.css';
import Nav from "./Components/Nav";
import NavDoc from "./Components/NavDoc";

//Menu
import MenuLogin from "./VistaMenu/MenuLogin";

//Curador 
import LoginCurador from "./VistaCurador/LoginCurador";
import RegistroCurador from "./VistaCurador/RegistroCurador";
import CrearCluster from "./VistaCurador/CrearCluster";
import CrearGenero from "./VistaCurador/CrearGenero";
import VerCluster from "./VistaCurador/VerCluster";
import ImportarGeneros  from "./VistaCurador/ImportarGeneros";
import CatalogoArtistas from "./VistaCurador/CatalogoArtistas";
import CrearArtista from "./VistaCurador/CrearArtista";
import MenuCurador from "./VistaCurador/MenuCurador";


function App() {
  
  const LayoutCurador = () => (
    <>
      <Nav/>
      <Outlet />
    </>
  );

  
  return (
    <BrowserRouter>
     
      <Routes>
        {/**Logins */}
          <Route path="/" element={<MenuLogin />} />
          <Route path="/logincurador" element={<LoginCurador />} />
          <Route path="/registercurador" element={<RegistroCurador />} />

        {/**Rutas Curador */}
        <Route element={<LayoutCurador />}>
          
          <Route path="/curador/crearcluster" element={<CrearCluster />} />
          <Route path="/curador/vercluster" element={<VerCluster />} />
          <Route path="/curador/menucurador" element={<MenuCurador />} />
          <Route path="/curador/creargenero" element={<CrearGenero />} />
          <Route path="/curador/importargeneros" element={<ImportarGeneros />} />
          <Route path="/curador/crearartista" element={<CrearArtista />} />
          <Route path="/curador/catalogoartistas" element={<CatalogoArtistas />} />

        </Route>

      </Routes>
    
    
    </BrowserRouter>

  )
}

export default App
