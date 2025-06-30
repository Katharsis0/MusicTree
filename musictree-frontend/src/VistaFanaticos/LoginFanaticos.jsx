import React, { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import Swal from 'sweetalert2';

import DivInput from '../Components/DivInput';

const api = import.meta.env.VITE_API_URL;

const LoginFanaticos = () => {
  const [data, setData] = useState([]);
  const navigate = useNavigate();

  const [values, setValues] = useState({
    username: '',
    password: '',
  });

  const [showPassword, setShowPassword] = useState(false);

  const login = async (e) => {
    e.preventDefault();

    if (!values.username || !values.password) {
      Swal.fire('Error', 'Debes completar ambos campos.', 'warning');
      return;
    }

    try {
      const res = await axios.get(`${api}/api/Fanaticos`);
      const fanaticos = res.data || [];

      const usuarioValido = fanaticos.find(f =>
        f.username === values.username && f.password === values.password
      );

      if (usuarioValido) {
        Swal.fire('Éxito', 'Inicio de sesión exitoso.', 'success');
        localStorage.setItem('fanaticoUsername', usuarioValido.username); // GUARDADO AQUÍ
        navigate('/fanaticos/menufanaticos');

      }else {
        Swal.fire('Error', 'Usuario o contraseña incorrectos.', 'error');
      }
    } catch (err) {
      console.error('Error al consultar fanaticos:', err);
      Swal.fire('Error', 'No se pudo completar el inicio de sesión.', 'error');
    }
  };

  return (
    <div className='container-fluid'>
      <div className='row mt-5'>
        <div className='col-md-4 offset-md-4'>
          <div className='card border border-dark'>
            <div className='card-header bg-dark border border-dark text-white'>
              LOGIN FANÁTICOS
            </div>
            <div className='card-body'>
              <form onSubmit={login}>
                <div className='input-group mb-3'>
                <input
                  type='text'
                  className='form-control'
                  placeholder='username'
                  required
                  value={values.username}
                  onChange={(e) => setValues({ ...values, username: e.target.value })}
                />
              </div>


                <div className='input-group mb-3'>
                  <input
                    type={showPassword ? 'text' : 'password'}
                    className='form-control'
                    placeholder='password'
                    required
                    value={values.password}
                    onChange={(e) =>
                      setValues({ ...values, password: e.target.value })
                    }
                  />
                  <button
                    type='button'
                    className='btn btn-outline-secondary'
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    <i
                      className={`fa-solid ${
                        showPassword ? 'fa-eye-slash' : 'fa-eye'
                      }`}
                    ></i>
                  </button>
                </div>

                <div className='d-grid col-10 mx-auto'>
                  <button className='btn btn-dark'>
                    <i className='fa-solid fa-door-open'></i> LOGIN
                  </button>
                </div>
              </form>

              <div className='mt-3'>
                <Link to='/registerfanaticos'>
                  <i className='fa-solid fa-user-plus'></i> REGISTRO
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginFanaticos;
