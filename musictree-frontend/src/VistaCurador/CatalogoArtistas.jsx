import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const CatalogoArtistas = () => {
  const [artistas, setArtistas] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios.get(`${api}/api/Artists`)
      .then(res => {
        const data = res.data || [];
        setArtistas((data.items || []).sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)));
        setError(null);
      })
      .catch(err => {
        console.error(err);
        setError('Ocurrió un error al cargar los artistas. Intente más tarde.');
      });
  }, []);

  return (
    <div className="container py-4">
      <h1>Catálogo de Artistas</h1>

      {error && (
        <div className="alert alert-danger">{error}</div>
      )}

      {artistas.length === 0 && !error ? (
        <div className="alert alert-info">No hay artistas registrados aún.</div>
      ) : (
        <div className="table-responsive mt-4">
          <table className="table table-striped table-bordered">
            <thead className="table-dark">
              <tr>
                <th>Identificador</th>
                <th>Nombre</th>
                <th>País</th>
                <th>Cantidad de Discos</th>
                <th>Fecha de Registro</th>
              </tr>
            </thead>
            <tbody>
              {artistas.map((artista) => (
                <tr key={artista.idArtista}>
                  <td>{artista.id}</td>
                  <td>{artista.name}</td>
                  <td>{artista.originCountry}</td>
                  <td>{artista.albumCount}</td>
                  <td>{new Date(artista.createdAt).toLocaleString()}</td>

                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="mt-3">
        <Link to="/curador/menucurador" className="btn btn-primary">Volver</Link>
      </div>
    </div>
  );
};

export default CatalogoArtistas;
