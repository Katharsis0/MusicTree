import { BrowserRouter, Routes, Route, Outlet } from "react-router-dom";
import 'bootstrap/dist/css/bootstrap.min.css';
import Nav from "./Components/Nav";
import NavDoc from "./Components/NavDoc";

//Menu
import MenuLogin from "./VistaMenu/MenuLogin";

//Curador 
import LoginCurador from "./VistaCurador/LoginCurador";
import RegistroCurador from "./VistaCurador/RegistroCurador";
import Reservacion from "./VistaCurador/Reservacion";
import RegistrarReservacion from "./VistaCurador/RegistrarReservacion";
import VerReservaciones from "./VistaCurador/VerReservaciones";
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
          
          <Route path="/curador/reservacion" element={<Reservacion />} />
          <Route path="/curador/reservacion/registrar/:id" element={<RegistrarReservacion />} />
          <Route path="/curador/reservaciones" element={<VerReservaciones />} />
          <Route path="/curador/menucurador" element={<MenuCurador />} />

        </Route>

      </Routes>
    
    
    </BrowserRouter>

  )
}

export default App
