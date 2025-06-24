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

    console.log("=== Enviando artista al backend ===");
    console.log("Id:", values.idArtista);
    console.log("Name:", values.nombre);
    console.log("Biography:", values.biografia);
    console.log("OriginCountry:", values.pais);
    console.log("IsActive:", values.activo);
    console.log("ActivityYears:", values.añosActividad);
    console.log("CreatedAt:", values.fechaCreacion);
    console.log("Genres:", values.generos);
    console.log("Subgenres:", values.subgeneros);
    console.log("Members:", values.miembros);
    console.log("Albums:", values.discos);
    console.log("CoverImage:", values.portada);
    console.log("ArtistRelatedGenres:", values.generos.map(g => ({
      GenreId: g,    
      InfluenceCoefficient: g.influenceCoefficient ?? 1.0
    })));

    console.log("ArtistRelatedSubgenres:", (values.subgeneros ?? []).map(sg => ({
      GenreId: sg,    
      InfluenceCoefficient: sg.influenceCoefficient ?? 1.0
    })));

    const formData = new FormData();

    // Campos básicos
    formData.append('Name', values.nombre);
    formData.append('Biography', values.biografia);
    formData.append('OriginCountry', values.pais);
    formData.append('ActivityYears', values.añosActividad);
    formData.append('CreatedAt', values.fechaCreacion);
    formData.append('IsActive', values.activo);

    const artistGenresPayload = values.generos.map(g => ({
    GenreId: g,
      InfluenceCoefficient: 1.0
    }));

    console.log("ArtistRelatedGenres:", artistGenresPayload);

    // Enviar como campos indexados:
    artistGenresPayload.forEach((g, index) => {
      formData.append(`ArtistRelatedGenres[${index}].GenreId`, g.GenreId);
      formData.append(`ArtistRelatedGenres[${index}].InfluenceCoefficient`, g.InfluenceCoefficient);
    });
    const artistSubgenresPayload = values.subgeneros.map(sg => ({
    GenreId: sg,                  
      InfluenceCoefficient: 1.0     
    }));

    console.log(">>> Enviando ArtistRelatedSubgenres:", artistSubgenresPayload);

    artistSubgenresPayload.forEach((sg, index) => {
      formData.append(`ArtistRelatedSubgenres[${index}].GenreId`, sg.GenreId);
      formData.append(`ArtistRelatedSubgenres[${index}].InfluenceCoefficient`, sg.InfluenceCoefficient);
    });


    // Otros campos
    formData.append('Members', JSON.stringify(values.miembros));
    formData.append('Albums', JSON.stringify(values.discos));

    if (values.portada) {
      formData.append('CoverImage', values.portada);
    }


    try {
      const response = await axios.post(`${api}/api/Artists`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      console.log("Artista creado con éxito:", response.data);
      Swal.fire("Éxito", "El artista se creó correctamente.", "success");

    } catch (error) {
      console.error("Error al crear artista:", error);

      if (error.response && error.response.data) {
        console.log("Detalles del error:", error.response.data);
      }

      Swal.fire("Error", "No se pudo crear el artista.", "error");
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
