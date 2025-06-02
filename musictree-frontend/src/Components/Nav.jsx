import {Link, useNavigate} from 'react-router-dom';

// Define el componente funcional Nav
const pacienteData = JSON.parse(localStorage.getItem('data-paciente'));
const pacienteCedula = pacienteData ? pacienteData.cedula : '';
const Nav = () => {
  const go = useNavigate();

   // Función para cerrar sesión de un administrador
  const logout = async() =>{
    localStorage.removeItem('data-paciente');
    localStorage.removeItem('userPaciente');
    go('/logincurador');
  
    
  }


  return (
    <nav className='navbar navbar-expand-lg navbar-white bg-info'> {/* Barra de navegación */}
      <div className='container-fluid'>
        <a className='navbar-brand'>Vista Curador</a> {/* Marca de navegación */}
        <button className='navbar-toggler' type='button' data-bs-toggle='collapse' data-bs-target='#nav' aria-controls='navbarSupportedContent'>
          <span className='navbar-toggler-icon'></span>
        </button>
      </div>
      {/* Renderiza las opciones de navegación según el tipo de usuario */}
      
      {true ? (
        <div className='collapse navbar-collapse' id='nav'>
          <ul className='navbar-nav mx-auto mb-2'>
            <li className='nav-item px-lg-5'>
              <button className='btn btn-info' onClick={logout}>LOGOUT</button>
            </li>
          </ul>
        </div>
      ) : ''}

    
      
    </nav>
  )
}

export default Nav