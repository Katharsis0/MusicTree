import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const BuscarArtistaFanaticos = () => {
  const [artistas, setArtistas] = useState([]);
  const [filtrados, setFiltrados] = useState([]);
  const [generos, setGeneros] = useState([]);
  const [subgeneros, setSubgeneros] = useState([]);
  const [generoSeleccionado, setGeneroSeleccionado] = useState('');
  const [subgeneroSeleccionado, setSubgeneroSeleccionado] = useState('');
  const [nombreBusqueda, setNombreBusqueda] = useState('');
  const [error, setError] = useState(null);

  // Cargar géneros una sola vez
  useEffect(() => {
    axios.get(`${api}/api/Genres`)
      .then(res => setGeneros(res.data.genres || []))
      .catch(err => console.error(err));
  }, []);

  // Cargar subgéneros cuando cambia el género seleccionado
  useEffect(() => {
    if (generoSeleccionado) {
      axios.get(`${api}/api/Genres/${generoSeleccionado}/subgenres`)
        .then(res => setSubgeneros(res.data || []))
        .catch(() => setSubgeneros([]));
    } else {
      setSubgeneros([]);
    }
  }, [generoSeleccionado]);

  // Buscar artistas cuando cambia el nombre de búsqueda
  useEffect(() => {
    const term = encodeURIComponent(nombreBusqueda);
    axios.get(`${api}/api/Artists/search?searchTerm=${term}&searchFields=name%2Cbiography%2CoriginCountry&exactMatch=false&caseSensitive=false&pageNumber=1&pageSize=50`)
      .then(res => {
        const data = res.data?.artists || [];
        setArtistas(data);
        setError(null);
      })
      .catch(err => {
        console.error(err);
        setError('Ocurrió un error al cargar los artistas. Intente más tarde.');
      });
  }, [nombreBusqueda]);

  // Filtrar localmente por género y subgénero
  useEffect(() => {
    const filtro = artistas.filter(artista => {
      const generoCoincide = !generoSeleccionado || artista.genreId === generoSeleccionado;
      const subgeneroCoincide = !subgeneroSeleccionado || (artista.subgenres?.some(s => s.id === subgeneroSeleccionado));
      return generoCoincide && subgeneroCoincide;
    });
    setFiltrados(filtro);
  }, [artistas, generoSeleccionado, subgeneroSeleccionado]);

  return (
    <div className="container py-4">
      <h1>Buscar Artista por Género</h1>

      <div className="mb-3">
        <label className="form-label">Género musical</label>
        <select className="form-select" value={generoSeleccionado} onChange={(e) => {
          setGeneroSeleccionado(e.target.value);
          setSubgeneroSeleccionado('');
        }}>
          <option value="">-- Seleccione un género --</option>
          {generos.map(g => (
            <option key={g.id} value={g.id}>{g.name}</option>
          ))}
        </select>
      </div>

      {subgeneros.length > 0 && (
        <div className="mb-3">
          <label className="form-label">Subgénero musical</label>
          <select className="form-select" value={subgeneroSeleccionado} onChange={(e) => setSubgeneroSeleccionado(e.target.value)}>
            <option value="">-- Todos los subgéneros --</option>
            {subgeneros.map(s => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
        </div>
      )}

      <div className="mb-3">
        <label className="form-label">Nombre del artista</label>
        <input
          type="text"
          className="form-control"
          placeholder="Ingrese el nombre o parte del nombre"
          value={nombreBusqueda}
          onChange={(e) => setNombreBusqueda(e.target.value)}
        />
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {filtrados.length === 0 && !error ? (
        <div className="alert alert-warning">No se encontraron coincidencias.</div>
      ) : (
        <div className="table-responsive mt-4">
          <table className="table table-striped table-bordered">
            <thead className="table-dark">
              <tr>
                <th>Nombre</th>
                <th>Cantidad de Discos</th>
                <th>Género</th>
                <th>Subgéneros</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtrados.map((artista) => (
                <tr key={artista.id}>
                  <td>{artista.name}</td>
                  <td>{artista.albumCount}</td>
                  <td>{/* Género no disponible en la respuesta */}N/A</td>
                  <td>{/* Subgéneros no disponibles en la respuesta */}Ninguno</td>
                  <td>
                    <Link to={`/fanaticos/perfilartista/${artista.id}`} className="btn btn-sm btn-info">
                      Ver Perfil
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="mt-3">
        <Link to="/fanaticos/menufanaticos" className="btn btn-primary">Volver</Link>
      </div>
    </div>
  );
};

export default BuscarArtistaFanaticos;
