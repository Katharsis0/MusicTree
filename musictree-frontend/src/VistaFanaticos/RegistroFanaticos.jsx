import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import countryList from 'react-select-country-list';


const RegistroFanaticos = () => {
  const [values, setValues] = useState({
    nickname: '',
    password: '',
    nombre: '',
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

  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    // Obtener géneros musicales desde API
    axios.get('http://localhost:5197/api/Genero')
      .then(res => setGeneros(res.data))
      .catch(err => console.error("Error al cargar géneros", err));
  }, []);

  const validarContrasena = (pwd) => {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,12}$/;
    return regex.test(pwd);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const { nickname, password, nombre, generosFavoritos, pais, avatar } = values;

    // Validaciones
    if (!nickname || !password || !nombre || generosFavoritos.length === 0 || !pais || !avatar) {
      setError('Debe completar todos los campos obligatorios.');
      return;
    }

    if (!validarContrasena(password)) {
      setError('La contraseña debe tener entre 8 y 12 caracteres e incluir mayúsculas, minúsculas y números.');
      return;
    }

    try {
      await axios.post('http://localhost:5197/api/Fanatico/registrar', values);
      alert('Registro exitoso. Ahora puede iniciar sesión.');
      navigate('/login');
    } catch (err) {
      if (err.response?.status === 409) {
        setError('El nombre de usuario ya existe. Intente con otro.');
      } else {
        setError('Error en el registro. Intente más tarde.');
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

        {error && <div className="alert alert-danger">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label>Nombre de Usuario (Nickname)</label>
            <input name="nickname" maxLength={30} onChange={handleChange} className="form-control" />
          </div>

          <div className="mb-3">
            <label>Contraseña</label>
            <input type="password" name="password" onChange={handleChange} className="form-control" />
          </div>

          <div className="mb-3">
            <label>Nombre</label>
            <input name="nombre" maxLength={100} onChange={handleChange} className="form-control" />
          </div>

          <div className="mb-3">
            <label>Géneros Musicales Favoritos</label>
            <div className="d-flex flex-wrap gap-2">
              {generos.map(g => (
                <div key={g.id}>
                  <input type="checkbox" id={`gen-${g.id}`} onChange={() => toggleGenero(g.id)} />
                  <label htmlFor={`gen-${g.id}`}> {g.nombre} </label>
                </div>
              ))}
            </div>
          </div>

          <div className='mb-2'>
            <label>País</label>
            <select className='form-select' value={values.pais}
              onChange={e => setValues({ ...values, pais: e.target.value })}>
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
