import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';

const CrearGenero = () => {
  const [values, setValues] = useState({
    nombre: '',
    desc: '',
    activo: true,
    color: '',
    año: '',
    pais: '',
    cluster: '',
    modo: 0.5,
    bpm_min: 85,
    bpm_max: 115,
    tono: -1,
    volumen: -40,
    tiempo: 4,
    duracion: 180,
    subgenero: '',
    padre: '',
  });

  const navigate = useNavigate();

  const handleSubmit = async (event) => {
    event.preventDefault();

    try {
      await axios.post('http://localhost:5197/api/Cluster/registrar_genero', values);
      console.log('Se creó el género correctamente');
      navigate('/curador/menucurador');
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <div className='d-flex w-100 vh-100 justify-content-center align-items-center bg-light'>
      <div className='w-50 border bg-white shadow px-5 pt-3 pb-5 rounded overflow-auto'>
        <h1>REGISTRO DE GÉNEROS/SUBGÉNEROS</h1>
        <form onSubmit={handleSubmit}>
          {/* Campos previos */}
          <div className='mb-2'>
            <label>Nombre</label>
            <input type='text' className='form-control'
              onChange={e => setValues({ ...values, nombre: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Descripción (opcional)</label>
            <input type='text' className='form-control'
              onChange={e => setValues({ ...values, desc: e.target.value })} />
          </div>

          <div className='mb-3 form-check form-switch'>
            <input className='form-check-input' type='checkbox' checked={values.activo}
              onChange={e => setValues({ ...values, activo: e.target.checked })} />
            <label className='form-check-label'>
              {values.activo ? 'Activo' : 'Desactivado'}
            </label>
          </div>

          <div className='mb-2'>
            <label>Color</label>
            <input type='number' className='form-control'
              onChange={e => setValues({ ...values, color: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Año</label>
            <input type='text' className='form-control'
              onChange={e => setValues({ ...values, año: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>País</label>
            <input type='text' className='form-control'
              onChange={e => setValues({ ...values, pais: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Cluster</label>
            <input type='text' className='form-control'
              onChange={e => setValues({ ...values, cluster: e.target.value })} />
          </div>

          {/* 🎵 Modo promedio */}
          <div className='mb-3'>
            <label>Modo promedio (0 = menor, 1 = mayor)</label>
            <input type='range' min='0' max='1' step='0.01' value={values.modo}
              onChange={e => setValues({ ...values, modo: parseFloat(e.target.value) })}
              className='form-range' />
            <input type='number' min='0' max='1' step='0.01' value={values.modo}
              onChange={e => setValues({ ...values, modo: parseFloat(e.target.value) })}
              className='form-control mt-1' />
          </div>

          {/* 🎵 BPM */}
          <div className='mb-2'>
            <label>Rango típico de BPM</label>
            <div className='d-flex gap-2'>
              <input type='number' className='form-control' placeholder='Mínimo'
                min='0' max='250' value={values.bpm_min}
                onChange={e => setValues({ ...values, bpm_min: parseInt(e.target.value) })} />
              <input type='number' className='form-control' placeholder='Máximo'
                min='0' max='250' value={values.bpm_max}
                onChange={e => setValues({ ...values, bpm_max: parseInt(e.target.value) })} />
            </div>
          </div>

          {/* 🎵 Tono */}
          <div className='mb-2'>
            <label>Tono musical dominante</label>
            <select className='form-control' value={values.tono}
              onChange={e => setValues({ ...values, tono: parseInt(e.target.value) })}>
              <option value='-1'>Sin tono claro</option>
              <option value='0'>C</option>
              <option value='1'>C♯ / D♭</option>
              <option value='2'>D</option>
              <option value='3'>D♯ / E♭</option>
              <option value='4'>E</option>
              <option value='5'>F</option>
              <option value='6'>F♯ / G♭</option>
              <option value='7'>G</option>
              <option value='8'>G♯ / A♭</option>
              <option value='9'>A</option>
              <option value='10'>A♯ / B♭</option>
              <option value='11'>B</option>
            </select>
          </div>

          {/* 🎵 Volumen */}
          <div className='mb-2'>
            <label>Volumen típico (dB entre -60 y 0)</label>
            <input type='number' className='form-control' min='-60' max='0' value={values.volumen}
              onChange={e => setValues({ ...values, volumen: parseFloat(e.target.value) })} />
          </div>

          {/* 🎵 Tiempo (Compás) */}
          <div className='mb-2'>
            <label>Tiempo de compás característico (2-8 o 0 si no tiene)</label>
            <input type='number' className='form-control' min='0' max='8' value={values.tiempo}
              onChange={e => setValues({ ...values, tiempo: parseInt(e.target.value) })} />
          </div>

          {/* 🎵 Duración */}
          <div className='mb-2'>
            <label>Duración promedio (segundos, máx 3600)</label>
            <input type='number' className='form-control' min='0' max='3600' value={values.duracion}
              onChange={e => setValues({ ...values, duracion: parseInt(e.target.value) })} />
          </div>

          {/* Botones */}
          <button type='submit' className='btn btn-success'>GUARDAR</button>
          <Link to="/curador/menucurador" className='btn btn-primary ms-3'>VOLVER</Link>
        </form>
      </div>
    </div>
  );
};

export default CrearGenero;
