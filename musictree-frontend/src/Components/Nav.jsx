import { Link, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';

const Nav = () => {
  const go = useNavigate();
  const [username, setUsername] = useState('');

  // Al cargar el componente, revisa si hay username en localStorage
  useEffect(() => {
    const storedUser = localStorage.getItem('fanaticoUsername');
    if (storedUser) {
      setUsername(storedUser);
    }
  }, []);

  const logout = () => {
    localStorage.removeItem('fanaticoUsername');
    setUsername('');
    go('');
  };

  return (
    <nav className='navbar navbar-expand-lg navbar-white bg-info px-4'>
      <div className='container-fluid'>
        <Link className='navbar-brand fw-bold text-dark' to="/">MusicTree</Link>
        <button className='navbar-toggler' type='button' data-bs-toggle='collapse' data-bs-target='#nav' aria-controls='navbarSupportedContent'>
          <span className='navbar-toggler-icon'></span>
        </button>
      </div>

      {username && (
        <div className='collapse navbar-collapse' id='nav'>
          <ul className='navbar-nav ms-auto mb-2 d-flex align-items-center'>
            <li className='nav-item'>
              <button className='btn btn-dark' onClick={logout}>LOGOUT</button>
            </li>
          </ul>
        </div>
      )}
    </nav>
  );
};

export default Nav;
