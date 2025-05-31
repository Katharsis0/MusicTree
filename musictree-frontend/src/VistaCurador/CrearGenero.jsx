import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import countryList from 'react-select-country-list';
import { v4 as uuidv4 } from 'uuid';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const CrearGenero = () => {
  const navigate = useNavigate();

  const [values, setValues] = useState({
    nombre: '',
    desc: '',
    activo: true,
    color: '',
    año: '',
    pais: '',
    cluster: '',
    modo: 0.5,
    bpmMin: '',
    bpmMax: '',
    tono: '',
    volumen: '',
    tiempo: '',
    duracion: '',
    esSubgenero: false,
    padre: '',
    influencias: [],
    fechaCreacion: new Date().toISOString(),
    idGenero: `G-${uuidv4().slice(0, 12)}`,
    idSubgenero: 'S-000000000000'
  });

  const [generos, setGeneros] = useState([]);
  const [clusters, setClusters] = useState([]);
  const [clusterError, setClusterError] = useState(null); 
  useEffect(() => {
    //axios.get('${api}/api/Cluster/listar_generos')
    axios.get(`${api}/api/Genres`)
      .then(res => setGeneros(res.data.genres || []))
      .catch(err => console.log(err));
    
    //axios.get('${api}/api/Cluster/listar_clusters')
    
    axios.get(`${api}/api/Clusters`)
      .then(res => {
        if (!Array.isArray(res.data)) {
          console.error("La respuesta del backend no es un array:", res.data);
          throw new Error("Formato de respuesta inesperado");
        }

        setClusters(res.data);
        setClusterError(null);
      })
      .catch(err => {
        console.error("Error al obtener clusters:", err);
        setClusterError("No se pudieron cargar los clusters. Intente más tarde.");
      });
}, []);




  const handleInfluenciasChange = (index, field, value) => {
    const newInfluencias = [...values.influencias];
    newInfluencias[index][field] = value;
    setValues({ ...values, influencias: newInfluencias });
  };

  const addInfluencia = () => {
    setValues({
      ...values,
      influencias: [...values.influencias, { generoRelacionado: '', valorInfluencia: 5 }]
    });
  };

const handleSubmit = async (event) => {
  event.preventDefault();

  const parseOrNull = (val) => {
    const parsed = parseInt(val);
    return isNaN(parsed) ? null : parsed;
  };

  // Validaciones locales
  if (!values.nombre || values.nombre.length < 3 || values.nombre.length > 30) {
    Swal.fire('Error', 'Debe ingresar un nombre entre 3 y 30 caracteres.', 'warning');
    return;
  }

  if (values.esSubgenero && !values.padre) {
    Swal.fire('Error', 'Debe seleccionar un género padre para el subgénero.', 'warning');
    return;
  }

  if (parseOrNull(values.bpmMin) < 0 || parseOrNull(values.bpmMax) > 250 || parseOrNull(values.bpmMin) > parseOrNull(values.bpmMax)) {
    Swal.fire('Error', 'El rango de BPM debe estar entre 0 y 250 y el mínimo no puede ser mayor que el máximo.', 'warning');
    return;
  }

  if (parseOrNull(values.tono) < -1 || parseOrNull(values.tono) > 11) {
    Swal.fire('Error', 'El tono dominante debe estar entre -1 y 11.', 'warning');
    return;
  }

  if (parseOrNull(values.volumen) < -60 || parseOrNull(values.volumen) > 0) {
    Swal.fire('Error', 'El volumen debe estar entre -60 y 0 DB.', 'warning');
    return;
  }

  if (parseOrNull(values.tiempo) < 0 || parseOrNull(values.tiempo) > 8) {
    Swal.fire('Error', 'El compás debe estar entre 0 y 8.', 'warning');
    return;
  }

  if (parseOrNull(values.duracion) < 0 || parseOrNull(values.duracion) > 3600) {
    Swal.fire('Error', 'La duración debe estar entre 0 y 3600 segundos.', 'warning');
    return;
  }

  try {

    let mgpc = null;

    if (values.esSubgenero) {
      const padre = generos.find(g => g.id === values.padre);
      const duracionPadre = padre?.musicalAttributes?.averageDuration;

      const bpmSuperior = parseInt(values.bpmMax);
      const duracionSubgenero = parseInt(values.duracion);

      if (!isNaN(bpmSuperior) && !isNaN(duracionSubgenero) && duracionPadre > 0) {
        mgpc = ((bpmSuperior * duracionSubgenero) / duracionPadre).toFixed(2);
        console.log(`MGPC calculado correctamente: (${bpmSuperior} * ${duracionSubgenero}) / ${duracionPadre} = ${mgpc}`);
      } else {
        console.warn(" No se pudo calcular MGPC: datos inválidos o género padre no encontrado.");
      }
    }

    const payload = {
      Name: values.nombre,
      Description: values.desc,
      IsSubgenre: values.esSubgenero,
      ParentGenreId: values.esSubgenero ? values.padre : null,
      ClusterId: values.esSubgenero ? null : values.cluster || null,
      GenreTipicalMode: parseFloat(values.modo),
      Volume: parseOrNull(values.volumen),
      CompasMetric: parseOrNull(values.tiempo),
      AvrgDuration: parseOrNull(values.duracion),
      Key: parseOrNull(values.tono),
      BpmLower: parseOrNull(values.bpmMin),
      BpmUpper: parseOrNull(values.bpmMax),
      Color: values.esSubgenero ? null : values.color,
      GenreCreationYear: parseOrNull(values.año),
      GenreOriginCountry: values.pais,
      //MGPC: values.esSubgenero ? parseFloat(mgpc) : null,
      RelatedGenres: values.influencias.map(inf => ({
        GenreId: inf.generoRelacionado,
        InfluenceStrength: parseOrNull(inf.valorInfluencia)
      }))
    };

    console.log("Payload enviado:", payload);

    const response = await axios.post(`${api}/api/Genres`, payload);




    if (response.data?.duplicado) {
      Swal.fire('Error', 'Ya existe una carta con esta combinación de género y subgénero.', 'warning');
      return;
    }

    Swal.fire('Éxito', 'Se creó el género correctamente.', 'success');
    navigate('/curador/menucurador');
  } catch (error) {
    Swal.fire('Error', 'No se pudo procesar la transacción. Intente más tarde.', 'error');
    console.error(error.response?.data || error);
  }
};



  const countries = countryList().getData();

  return (
    <div className='d-flex flex-column align-items-center bg-light min-vh-100 py-4 overflow-auto'>
      <div className='w-75 border bg-white shadow px-5 pt-3 pb-5 rounded overflow-auto'>
        <h1>Registro de Género/Subgénero Musical</h1>
        <form onSubmit={handleSubmit}>

          <div className='mb-2'>
            <label>Nombre</label>
            <input type='text' className='form-control' value={values.nombre}
              onChange={e => setValues({ ...values, nombre: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Descripción</label>
            <textarea className='form-control' maxLength='1000' value={values.desc}
              onChange={e => setValues({ ...values, desc: e.target.value })}></textarea>
          </div>

          <div className='form-check form-switch mb-3'>
            <input className='form-check-input' type='checkbox' checked={values.activo}
              onChange={e => setValues({ ...values, activo: e.target.checked })} />
            <label className='form-check-label'>Estado: {values.activo ? 'Activo' : 'Inactivo'}</label>
          </div>

          <div className='form-check mb-3'>
            <input className='form-check-input' type='checkbox' checked={values.esSubgenero}
              onChange={e => setValues({ ...values, esSubgenero: e.target.checked, padre: '', color: '' })} />
            <label className='form-check-label'>¿Es Subgénero?</label>
          </div>

          {values.esSubgenero && (
            <div className='mb-2'>
              <label>Género Padre</label>
              <select className='form-select' value={values.padre}
                onChange={e => setValues({ ...values, padre: e.target.value })}>
                <option value=''>Seleccione un género</option>
                {Array.isArray(generos) && generos
                  .filter(g => !g.isSubgenre)
                  .map(g => (
                    <option key={g.id} value={g.id}>{g.name}</option>
                ))}

              </select>
            </div>
          )}

          {!values.esSubgenero && (
            <div className='mb-2'>
              <label>Color</label>
              <input type='text' className='form-control' value={values.color}
                onChange={e => setValues({ ...values, color: e.target.value })} />
            </div>
          )}

          <div className='mb-2'>
            <label>Año de creación</label>
            <input type='number' className='form-control' value={values.año}
              onChange={e => setValues({ ...values, año: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>País de origen</label>
            <select className='form-select' value={values.pais}
              onChange={e => setValues({ ...values, pais: e.target.value })}>
              <option value=''>Seleccione un país</option>
              {countries.map((country, index) => (
                <option key={index} value={country.label}>{country.label}</option>
              ))}
            </select>
          </div>

          <div className='mb-2'>
            <label>Clúster asociado (opcional)</label>

            {clusterError && (
              <div className="alert alert-danger" role="alert">
                {clusterError}
              </div>
            )}

            <select className='form-select' value={values.cluster}
              onChange={e => setValues({ ...values, cluster: e.target.value })}>
              <option value=''>Sin clúster</option>
              {clusters.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>

          </div>

          <div className='mb-2'>
            <label>Modo promedio (0.00 a 1.00)</label>
            <input type='number' step='0.01' min='0' max='1' className='form-control'
              value={values.modo} onChange={e => setValues({ ...values, modo: parseFloat(e.target.value) })} />
          </div>

          <div className='mb-2'>
            <label>Rango de BPM</label>
            <div className='d-flex gap-2'>
              <input type='number' placeholder='Mín' min='0' max='250' className='form-control'
                value={values.bpmMin} onChange={e => setValues({ ...values, bpmMin: e.target.value })} />
              <input type='number' placeholder='Máx' min='0' max='250' className='form-control'
                value={values.bpmMax} onChange={e => setValues({ ...values, bpmMax: e.target.value })} />
            </div>
          </div>

          <div className='mb-2'>
            <label>Tono dominante (0-11, -1 si no aplica)</label>
            <input type='number' min='-1' max='11' className='form-control'
              value={values.tono} onChange={e => setValues({ ...values, tono: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Volumen típico (entre -60 y 0 DB)</label>
            <input type='number' min='-60' max='0' className='form-control'
              value={values.volumen} onChange={e => setValues({ ...values, volumen: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Tiempo de compás (2 a 8 o 0 si no aplica)</label>
            <input type='number' min='0' max='8' className='form-control'
              value={values.tiempo} onChange={e => setValues({ ...values, tiempo: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label>Duración promedio (en segundos, 0 a 3600)</label>
            <input type='number' min='0' max='3600' className='form-control'
              value={values.duracion} onChange={e => setValues({ ...values, duracion: e.target.value })} />
          </div>

          <hr />

          <h5>Relaciones de Influencia</h5>
          {values.influencias.map((inf, index) => (
            <div key={index} className='mb-2 d-flex gap-2'>
              <select className='form-select' value={inf.generoRelacionado}
                onChange={e => handleInfluenciasChange(index, 'generoRelacionado', e.target.value)}>
                <option value=''>Seleccione un género</option>
                {generos.map(g => (
                  <option key={g.idGenero} value={g.idGenero}>{g.nombre}</option>
                ))}
              </select>
              <input type='number' min='1' max='10' className='form-control' value={inf.valorInfluencia}
                onChange={e => handleInfluenciasChange(index, 'valorInfluencia', e.target.value)} />
            </div>
          ))}
          <div className="d-flex justify-content-start gap-3 mt-4">
            <button type="button" className="btn btn-secondary" onClick={addInfluencia}>Agregar Influencia</button>
            <button type="submit" className="btn btn-success">GUARDAR</button>
            <Link to="/curador/menucurador" className="btn btn-primary">VOLVER</Link>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CrearGenero;