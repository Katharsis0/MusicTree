import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import Swal from 'sweetalert2';
import countryList from 'react-select-country-list';

const api = import.meta.env.VITE_API_URL;

const RegistroFanaticos = () => {
  const [values, setValues] = useState({
    username: '',
    password: '',
    name: '',
    generosFavoritos: [],
    pais: '',
    avatar: ''
  });

  const [generos, setGeneros] = useState([]);
  const [avatares, setAvatares] = useState([
    "/avatars/1.webp",
    "/avatars/2.webp",
    "/avatars/7.png",
    "/avatars/3.webp",
    "/avatars/4.webp",
    "/avatars/5.webp",
    "/avatars/6.webp"
  ]);

  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    axios.get(`${api}/api/Genres`)
      .then(res => setGeneros(res.data.genres || []))
      .catch(err => console.error(err));
  }, []);

  const validarContrasena = (pwd) => {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,12}$/;
    return regex.test(pwd);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const { username, password, name, pais, avatar, generosFavoritos } = values;

    if (!username || !password || !name || generosFavoritos.length === 0 || !pais || !avatar) {
      Swal.fire('Campos incompletos', 'Debe completar todos los campos obligatorios.', 'warning');
      return;
    }

    if (!validarContrasena(password)) {
      Swal.fire(
        'Contraseña inválida',
        'Debe tener entre 8 y 12 caracteres, incluir mayúsculas, minúsculas y números.',
        'warning'
      );
      return;
    }

    try {
      await axios.post(`${api}/api/Fanaticos`, values);
      Swal.fire('Registro exitoso', 'Ahora puede iniciar sesión.', 'success').then(() => {
        navigate('/loginfanaticos');
      });
    } catch (err) {
      console.error(err);
      if (err.response?.status === 409) {
        Swal.fire('Usuario existente', 'El nombre de usuario ya existe. Intente con otro.', 'error');
      } else {
        Swal.fire('Error', 'Ocurrió un error durante el registro. Intente más tarde.', 'error');
      }
    }
  };

  const handleChange = (e) => {
    setValues({ ...values, [e.target.name]: e.target.value });
  };

  const toggleGenero = (id) => {
    setValues(prev => {
      const nuevos = prev.generosFavoritos.includes(id)
        ? prev.generosFavoritos.filter(g => g !== id)
        : [...prev.generosFavoritos, id];
      return { ...prev, generosFavoritos: nuevos };
    });
  };

  const countries = countryList().getData();

  return (
    <div className="container mt-5">
      <div className="card shadow p-4">
        <h2 className="mb-4">Registrar Fanático</h2>

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label>Nombre de Usuario (Nickname)</label>
            <input
              name="username"
              maxLength={30}
              value={values.username}
              onChange={handleChange}
              className="form-control"
            />
          </div>

          <div className="mb-3">
            <label>Contraseña</label>
            <div className="input-group">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
                className="form-control"
                value={values.password}
                onChange={handleChange}
              />
              <button
                type="button"
                className="btn btn-outline-secondary"
                onClick={() => setShowPassword(prev => !prev)}
                tabIndex={-1}
              >
                <i className={`fa ${showPassword ? 'fa-eye-slash' : 'fa-eye'}`}></i>
              </button>
            </div>
          </div>

          <div className="mb-3">
            <label>Nombre</label>
            <input
              name="name"
              maxLength={100}
              value={values.name}
              onChange={handleChange}
              className="form-control"
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Géneros Musicales Favoritos</label>
            <div className="form-check-group d-flex flex-column gap-1">
              {generos.filter(g => !g.isSubgenre).map(g => (
                <div key={g.id} className="form-check">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    id={`gen-${g.id}`}
                    checked={values.generosFavoritos.includes(g.id)}
                    onChange={() => toggleGenero(g.id)}
                  />
                  <label className="form-check-label" htmlFor={`gen-${g.id}`}>
                    {g.name}
                  </label>
                </div>
              ))}
            </div>
          </div>

          <div className='mb-2'>
            <label>País</label>
            <select
              className='form-select'
              value={values.pais}
              onChange={e => setValues({ ...values, pais: e.target.value })}
            >
              <option value=''>Seleccione un país</option>
              {countries.map((country, index) => (
                <option key={index} value={country.label}>{country.label}</option>
              ))}
            </select>
          </div>

          <div className="mb-3">
            <label>Selecciona un Avatar</label>
            <div className="d-flex gap-3 flex-wrap">
              {avatares.map((url, index) => (
                <img
                  key={index}
                  src={url}
                  alt={`avatar-${index}`}
                  className={`rounded-circle border ${values.avatar === url ? 'border-primary border-3' : ''}`}
                  style={{ width: 60, height: 60, cursor: 'pointer' }}
                  onClick={() => setValues({ ...values, avatar: url })}
                />
              ))}
            </div>
          </div>

          <div className="d-flex justify-content-between mt-4">
            <button type="submit" className="btn btn-success">Registrarse</button>
            <Link to="/loginfanaticos" className="btn btn-secondary">Volver al login</Link>
          </div>
        </form>
      </div>
    </div>
  );
};

export default RegistroFanaticos;
