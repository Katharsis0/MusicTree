import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
const api = import.meta.env.VITE_API_URL;

const CrearCluster = () => {
  const [values, setValues] = useState({
    nombre: '',
    desc: '',
    activo: true
  });

  const navigate = useNavigate();

  const handleSubmit = async (event) => {
    event.preventDefault();

    // Validación local
    if (!values.nombre || values.nombre.length < 3 || values.nombre.length > 30) {
      alert('El nombre debe tener entre 3 y 30 caracteres.');
      return;
    }

    try {
      // Enviar solo lo que espera el backend
      await axios.post(`${api}/api/Clusters`, {
        Name: values.nombre,
        Description: values.desc
      });

      console.log('Se creó el clúster correctamente');
      navigate('/curador/menucurador');
    } catch (err) {
      console.error('Error al crear el clúster:', err);
      alert('No se pudo crear el clúster. Verifica los campos e intenta de nuevo.');
    }
  };

  return (
    <div className='d-flex w-100 vh-100 justify-content-center align-items-center bg-light'>
      <div className='w-50 border bg-white shadow px-5 pt-3 pb-5 rounded'>
        <h1>Crear Clúster</h1>
        <form onSubmit={handleSubmit}>
          <div className='mb-2'>
            <label htmlFor='nombre'>Nombre (requerido): </label>
            <input
              type='text'
              name='nombre'
              className='form-control'
              placeholder='Ingrese el nombre'
              onChange={e => setValues({ ...values, nombre: e.target.value })}
              required
            />
          </div>

          <div className='mb-2'>
            <label htmlFor='desc'>Descripción (opcional)</label>
            <input
              type='text'
              name='desc'
              className='form-control'
              placeholder='Ingrese la descripción'
              onChange={e => setValues({ ...values, desc: e.target.value })}
            />
          </div>

          <div className='mb-3 form-check form-switch'>
            <input
              className='form-check-input'
              type='checkbox'
              id='activoSwitch'
              checked={values.activo}
              onChange={e => setValues({ ...values, activo: e.target.checked })}
            />
            <label className='form-check-label' htmlFor='activoSwitch'>
              {values.activo ? 'Activo' : 'Desactivado'}
            </label>
          </div>

          <button type='submit' className='btn btn-success'>GUARDAR</button>
          <Link to="/curador/menucurador" className='btn btn-primary ms-3'>VOLVER</Link>
        </form>
      </div>
    </div>
  );
};

export default CrearCluster;
