import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate, useParams, Link } from 'react-router-dom';
import Swal from 'sweetalert2';
import './PerfilArtista.css';

const api = import.meta.env.VITE_API_URL;

const PerfilArtista = () => {
  const { id } = useParams();
  const [artista, setArtista] = useState(null);
  const [error, setError] = useState(null);
  const [tab, setTab] = useState('discografia');
  const [calificacion, setCalificacion] = useState(0);
  const [comentario, setComentario] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    axios.get(`${api}/api/Artists/${id}`)
      .then(res => setArtista(res.data))
      .catch(err => {
        console.error(err);
        Swal.fire('Error', 'No se pudo cargar el perfil. Intente más tarde.', 'error');
        setError('Error al cargar el perfil');
      });
  }, [id]);

  const handleRating = (valor) => {
    setCalificacion(valor);
  };

  const enviarCalificacion = () => {
    axios.post(`${api}/api/Artists/${id}/rate`, { rating: calificacion, comment: comentario })
      .then(() => {
        Swal.fire('¡Gracias!', 'Tu calificación ha sido enviada.', 'success');
      })
      .catch(() => {
        Swal.fire('Error', 'No se pudo enviar la calificación. Intenta más tarde.', 'error');
      });
  };

  if (error) return null;
  if (!artista) return <p>Cargando...</p>;

  return (
    <div className="container mt-4">
      <div className="d-flex mb-3">
        <img src={artista.coverImageUrl} alt="Portada" className="rounded me-3" width={120} height={120} />
        <div>
          <h3>{artista.name}</h3>
          <p><strong>⭐ {artista.ratingAverage || 'N/A'}</strong> ({artista.ratingCount || 0} calificaciones)</p>
          <div className="d-flex flex-wrap gap-3">
            <span><strong>ID:</strong> {artista.id}</span>
            <span><strong>Género:</strong> {artista.genre?.name}</span>
            <span><strong>Subgéneros:</strong> {artista.subgenres?.map(s => s.name).join(', ') || 'No disponible'}</span>
            <span><strong>País:</strong> {artista.country}</span>
            <span><strong>Actividad:</strong> {artista.yearsActive}</span>
            <span><strong>Fecha de creación:</strong> {new Date(artista.createdAt).toLocaleDateString()}</span>
          </div>
          <p><strong>Biografía:</strong> {artista.biography || 'No disponible'}</p>
        </div>
      </div>

      <div className="tabs-bar">
        {['discografia', 'miembros', 'calificaciones', 'comentarios', 'eventos', 'fotos'].map((t) => (
          <span
            key={t}
            className={`tab-item ${tab === t ? 'active' : ''}`}
            onClick={() => setTab(t)}
          >
            {t.charAt(0).toUpperCase() + t.slice(1)}
          </span>
        ))}
      </div>

      {tab === 'discografia' && (
        <div className="tab-content">
          {artista.albums?.length > 0 ? (
            <table className="table">
              <thead>
                <tr>
                  <th>Portada</th>
                  <th>ID Álbum</th>
                  <th>Título</th>
                  <th>Fecha de lanzamiento</th>
                  <th>Duración</th>
                </tr>
              </thead>
              <tbody>
                {artista.albums.map(a => (
                  <tr key={a.id}>
                    <td><img src={a.coverImageUrl} alt={a.title} width={50} /></td>
                    <td>{a.id}</td>
                    <td>{a.title}</td>
                    <td>{new Date(a.releaseDate).toLocaleDateString()}</td>
                    <td>{a.totalDuration}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : <p>No hay álbumes disponibles.</p>}
        </div>
      )}

      {tab === 'miembros' && (
        <div className="tab-content">
          {artista.members?.length > 0 ? (
            <ul>
              {artista.members.map(m => (
                <li key={m.id}>{m.fullName} - {m.instrument} ({m.period})</li>
              ))}
            </ul>
          ) : <p>No hay miembros registrados.</p>}
        </div>
      )}
      {tab === 'calificaciones' && (
        <div className="tab-content">
          <div className="d-flex mb-3">
            {[1, 2, 3, 4, 5].map(n => (
              <span
                key={n}
                style={{ cursor: 'pointer', color: n <= calificacion ? 'gold' : 'gray', fontSize: '1.5rem' }}
                onClick={() => handleRating(n)}
              >★</span>
            ))}
          </div>
          <button className="btn btn-primary mb-4" onClick={() => {
            axios.post(`${api}/api/Artists/${id}/rate`, { rating: calificacion })
              .then(() => Swal.fire('¡Gracias!', 'Tu calificación ha sido enviada.', 'success'))
              .catch(() => Swal.fire('Error', 'No se pudo enviar la calificación. Intenta más tarde.', 'error'));
          }}>
            Enviar Calificación
          </button>

          <h6>Calificaciones anteriores</h6>
          {artista.ratings?.length > 0 ? (
            <ul>
              {artista.ratings.map((r, i) => (
                <li key={i}>
                  <strong>{r.userName}</strong>: {'★'.repeat(r.value)}{'☆'.repeat(5 - r.value)}
                </li>
              ))}
            </ul>
          ) : <p>No hay calificaciones disponibles.</p>}
        </div>
      )}

      {tab === 'comentarios' && (
        <div className="tab-content">
          <h6>Comentarios</h6>
          <textarea
            placeholder="¿Qué opinas de este artista?"
            value={comentario}
            onChange={(e) => setComentario(e.target.value)}
            className="form-control mb-2"
          ></textarea>
          <button className="btn btn-primary mb-3" onClick={() => {
            axios.post(`${api}/api/Artists/${id}/comment`, { text: comentario })
              .then(() => {
                Swal.fire('¡Gracias!', 'Tu comentario ha sido enviado.', 'success');
                setComentario('');
                // Idealmente: recargar comentarios desde el backend
              })
              .catch(() => {
                Swal.fire('Error', 'No se pudo enviar el comentario. Intenta más tarde.', 'error');
              });
          }}>
            Comentar
          </button>

          {artista.comments?.length > 0 ? (
            <ul>{artista.comments.map((c, i) => (
              <li key={i}><strong>{c.userName || 'Anónimo'}:</strong> {c.text}</li>
            ))}</ul>
          ) : <p>No hay comentarios disponibles.</p>}
        </div>
      )}


      {tab === 'eventos' && (
        <div className="tab-content">
          {artista.events?.length > 0 ? (
            <table className="table">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Lugar de evento</th>
                </tr>
              </thead>
              <tbody>
                {artista.events.map((e, i) => (
                  <tr key={i}>
                    <td>{new Date(e.date).toLocaleDateString()}</td>
                    <td>{e.name}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : <p>No hay eventos disponibles.</p>}
        </div>
      )}

      {tab === 'fotos' && (
        <div className="tab-content">
          {artista.photos?.length > 0 ? (
            <div className="d-flex gap-2 flex-wrap">
              {artista.photos.map((p, i) => (
                <img key={i} src={p.url} alt="Foto" style={{ width: 100 }} />
              ))}
            </div>
          ) : <p>No hay fotos disponibles.</p>}
        </div>
      )}

      <div className="mt-3">
          <Link to="/fanaticos/buscarartistafanaticos" className="btn btn-primary">Volver</Link>
        </div>
    </div>
  );
};

export default PerfilArtista;