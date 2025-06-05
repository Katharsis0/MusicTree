import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import countryList from 'react-select-country-list';
import { v4 as uuidv4 } from 'uuid';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const CrearArtista = () => {
  const navigate = useNavigate();
  const [generos, setGeneros] = useState([]);
  const countries = countryList().getData();

  const [values, setValues] = useState({
    idArtista: `A-${uuidv4().slice(0, 12)}`,
    nombre: '',
    biografia: '',
    pais: '',
    activo: true,
    fechaCreacion: new Date().toISOString(),
    añosActividad: '',
    generos: [],
    subgeneros: [],
    miembros: [{ nombre: '', instrumento: '', periodo: '' }],
    discos: [{ titulo: '', fecha: '', imagen: null, duracion: '' }],
    portada: null,
  });

  useEffect(() => {
    axios.get(`${api}/api/Genres`)
      .then(res => setGeneros(res.data.genres || []))
      .catch(err => console.error(err));
  }, []);

  const handleMiembroChange = (index, field, value) => {
    const nuevos = [...values.miembros];
    nuevos[index][field] = value;
    setValues({ ...values, miembros: nuevos });
  };

  const addMiembro = () => {
    setValues({ ...values, miembros: [...values.miembros, { nombre: '', instrumento: '', periodo: '' }] });
  };

  const handleDiscoChange = (index, field, value) => {
    const nuevos = [...values.discos];
    nuevos[index][field] = value;
    setValues({ ...values, discos: nuevos });
  };

  const addDisco = () => {
    setValues({
      ...values,
      discos: [...values.discos, { titulo: '', fecha: '', imagen: null, duracion: '' }]
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!values.nombre || values.nombre.length < 3 || values.nombre.length > 100) {
      Swal.fire('Error', 'El nombre debe tener entre 3 y 100 caracteres.', 'warning');
      return;
    }

    if (!values.pais) {
      Swal.fire('Error', 'Debe seleccionar un país de origen.', 'warning');
      return;
    }

    if (!values.portada) {
      Swal.fire('Error', 'Debe subir una imagen de portada (JPEG, 800x800px, máx 5MB).', 'warning');
      return;
    }

    const formData = new FormData();
    formData.append('idArtista', values.idArtista);
    formData.append('nombre', values.nombre);
    formData.append('biografia', values.biografia);
    formData.append('pais', values.pais);
    formData.append('activo', values.activo);
    formData.append('fechaCreacion', values.fechaCreacion);
    formData.append('añosActividad', values.añosActividad);
    formData.append('generos', JSON.stringify(values.generos));
    formData.append('subgeneros', JSON.stringify(values.subgeneros));
    formData.append('miembros', JSON.stringify(values.miembros));
    formData.append('discos', JSON.stringify(values.discos));
    formData.append('portada', values.portada);

    try {
      await axios.post(`${api}/api/Artists`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      Swal.fire('Éxito', 'Artista creado correctamente.', 'success');
      navigate('/curador/menucurador');
    } catch (err) {
      Swal.fire('Error', 'No se pudo registrar el artista. Intente más tarde.', 'error');
      console.error(err);
    }
  };

  return (
    <div className='d-flex flex-column align-items-center bg-light min-vh-100 py-4'>
      <div className='w-75 border bg-white shadow px-5 pt-3 pb-5 rounded overflow-auto'>
        <h1>Registro de Artista</h1>
        <form onSubmit={handleSubmit}>
          <div className='mb-2'>
            <label>Nombre</label>
            <input type='text' className='form-control' value={values.nombre}
              onChange={e => setValues({ ...values, nombre: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Biografía</label>
            <textarea className='form-control' maxLength='2000' value={values.biografia}
              onChange={e => setValues({ ...values, biografia: e.target.value })}></textarea>
          </div>

          <div className='mb-2'>
            <label>País de origen</label>
            <select className='form-select' value={values.pais}
              onChange={e => setValues({ ...values, pais: e.target.value })}>
              <option value=''>Seleccione un país</option>
              {countries.map((c, i) => (
                <option key={i} value={c.label}>{c.label}</option>
              ))}
            </select>
          </div>

          <div className='mb-2'>
            <label>Años de actividad</label>
            <input type='text' className='form-control' placeholder='Ej: 1990–2001, 2010–presente'
              value={values.añosActividad}
              onChange={e => setValues({ ...values, añosActividad: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Géneros</label>
            <select multiple className='form-select' value={values.generos}
              onChange={e => setValues({ ...values, generos: Array.from(e.target.selectedOptions, opt => opt.value) })}>
              {generos.filter(g => !g.isSubgenre).map(g => (
                <option key={g.id} value={g.id}>{g.name}</option>
              ))}
            </select>
          </div>

          <div className='mb-2'>
            <label>Subgéneros</label>
            <select multiple className='form-select' value={values.subgeneros}
              onChange={e => setValues({ ...values, subgeneros: Array.from(e.target.selectedOptions, opt => opt.value) })}>
              {generos.filter(g => g.isSubgenre).map(g => (
                <option key={g.id} value={g.id}>{g.name}</option>
              ))}
            </select>
          </div>

          <div className='mb-3'>
            <label>Miembros</label>
            {values.miembros.map((m, i) => (
              <div key={i} className='d-flex gap-2 mb-2'>
                <input type='text' className='form-control' placeholder='Nombre'
                  value={m.nombre} onChange={e => handleMiembroChange(i, 'nombre', e.target.value)} />
                <input type='text' className='form-control' placeholder='Instrumento'
                  value={m.instrumento} onChange={e => handleMiembroChange(i, 'instrumento', e.target.value)} />
                <input type='text' className='form-control' placeholder='Período'
                  value={m.periodo} onChange={e => handleMiembroChange(i, 'periodo', e.target.value)} />
              </div>
            ))}
            <button type='button' className='btn btn-outline-secondary' onClick={addMiembro}>Agregar Miembro</button>
          </div>

          <div className='mb-3'>
            <label>Discografía</label>
            {values.discos.map((d, i) => (
              <div key={i} className='d-flex flex-wrap gap-2 mb-2'>
                <input type='text' className='form-control' placeholder='Título'
                  value={d.titulo} onChange={e => handleDiscoChange(i, 'titulo', e.target.value)} />
                <input type='date' className='form-control'
                  value={d.fecha} onChange={e => handleDiscoChange(i, 'fecha', e.target.value)} />
                <input type='number' className='form-control' placeholder='Duración (seg)'
                  value={d.duracion} onChange={e => handleDiscoChange(i, 'duracion', e.target.value)} />
                <input type='file' accept='image/jpeg' className='form-control'
                  onChange={e => handleDiscoChange(i, 'imagen', e.target.files[0])} />
              </div>
            ))}
            <button type='button' className='btn btn-outline-secondary' onClick={addDisco}>Agregar Disco</button>
          </div>

          <div className='mb-3'>
            <label>Portada del artista (JPEG, 800x800px, máx 5MB)</label>
            <input type='file' accept='image/jpeg' className='form-control'
              onChange={e => setValues({ ...values, portada: e.target.files[0] })} />
          </div>

          <div className='d-flex gap-3 mt-4'>
            <button type='submit' className='btn btn-success'>GUARDAR</button>
            <Link to="/curador/menucurador" className="btn btn-primary">VOLVER</Link>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CrearArtista;
