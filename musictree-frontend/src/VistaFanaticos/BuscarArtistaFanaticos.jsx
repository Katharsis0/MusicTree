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

  useEffect(() => {
    axios.get(`${api}/api/Artists`)
      .then(res => {
        const data = res.data?.items || [];
        setArtistas(data);
        setFiltrados(data);
        setError(null);
      })
      .catch(err => {
        console.error(err);
        setError('Ocurrió un error al cargar los artistas. Intente más tarde.');
      });

    axios.get(`${api}/api/Genres`)
      .then(res => setGeneros(res.data.genres || []))
      .catch(err => console.error(err));
  }, []);

  useEffect(() => {
    if (generoSeleccionado) {
      axios.get(`${api}/api/Genres/${generoSeleccionado}/subgenres`)
        .then(res => {
          setSubgeneros(res.data || []);
        })
        .catch(err => {
          console.error(err);
          setSubgeneros([]);
        });
    } else {
      setSubgeneros([]);
    }
  }, [generoSeleccionado]);

  useEffect(() => {
    filtrarArtistas();
  }, [generoSeleccionado, subgeneroSeleccionado, nombreBusqueda]);

  const filtrarArtistas = () => {
    const filtro = artistas.filter((artista) => {
      const nombre = artista.name?.toLowerCase() || '';
      const nombreCoincide = nombre.includes(nombreBusqueda.toLowerCase());
      const generoCoincide = !generoSeleccionado || artista.genreId === generoSeleccionado;
      const subgeneroCoincide = !subgeneroSeleccionado || (artista.subgenres?.some(sub => sub.id === subgeneroSeleccionado));
      return nombreCoincide && generoCoincide && subgeneroCoincide;
    });

    setFiltrados(filtro);
  };

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
              </tr>
            </thead>
            <tbody>
              {filtrados.map((artista) => (
                <tr key={artista.id}>
                  <td>{artista.name}</td>
                  <td>{artista.albumCount}</td>
                  <td>{artista.genre?.name || 'N/A'}</td>
                  <td>{artista.subgenres?.map(s => s.name).join(', ') || 'Ninguno'}</td>
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
