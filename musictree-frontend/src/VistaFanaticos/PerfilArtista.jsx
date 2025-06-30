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
  const [calificaciones, setCalificaciones] = useState([]); // <- nuevo estado para calificaciones
  const [comentario, setComentario] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    // Carga perfil artista
    axios.get(`${api}/api/Artists/${id}`)
      .then(res => setArtista(res.data))
      .catch(err => {
        console.error(err);
        Swal.fire('Error', 'No se pudo cargar el perfil. Intente más tarde.', 'error');
        setError('Error al cargar el perfil');
      });

    // Carga calificaciones desde /api/Fanaticos/calificar/{artistId}
    axios.get(`${api}/api/Fanaticos/calificar/${id}`)
      .then(res => {
        setCalificaciones(res.data);
      })
      .catch(err => {
        console.error('Error al cargar calificaciones:', err);
        setCalificaciones([]); // O vacío si falla
      });
  }, [id]);

  const handleRating = (valor) => {
    setCalificacion(valor);
  };

  const enviarCalificacion = async () => {
    const username = localStorage.getItem('fanaticoUsername');
    if (calificacion === 0) {
      Swal.fire('Error', 'Por favor selecciona una calificación.', 'error');
      return;
    }

    try {
        await axios.post(`${api}/api/Fanaticos/calificar`, {
        username: username,
        artistID: id,
        calificacion: calificacion
      });

      Swal.fire({
        icon: 'success',
        title: 'Calificación enviada',
        text: 'Se calificó correctamente.',
        confirmButtonColor: '#28a745'
      });

      // Actualizar lista de calificaciones para reflejar el cambio
      const res = await axios.get(`${api}/api/Fanaticos/calificar/${id}`);
      setCalificaciones(res.data);

      // Reset calificación para nuevo input si quieres
      setCalificacion(0);

    } catch (err) {
      console.error('Error al calificar:', err);
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: 'No se pudo enviar la calificación. Intenta más tarde.',
        confirmButtonColor: '#dc3545'
      });
    }
  };

  // Función para mostrar estrellas (con relleno y vacío)
  const mostrarEstrellas = (valor) => {
    const totalEstrellas = 5;
    return (
      <>
        {[...Array(totalEstrellas)].map((_, i) => (
          <span key={i} style={{ color: i < valor ? 'gold' : 'lightgray', fontSize: '1.2rem' }}>
            ★
          </span>
        ))}
      </>
    );
  };

  if (error) return null;
  if (!artista) return <p>Cargando...</p>;

  return (
    <div className="container mt-4">
      {/* Información artista (sin cambios) */}
      <div className="d-flex mb-3">
        <img src={artista.coverImageUrl} alt="Portada" className="rounded me-3" width={120} height={120} />
        <div>
          <h3>{artista.name}</h3>
          <p><strong>⭐ {artista.ratingAverage || 'N/A'}</strong> ({artista.ratingCount || 0} calificaciones)</p>
          <div className="d-flex flex-wrap gap-3">
            <span><strong>ID:</strong> {artista.id}</span>
            <span><strong>Géneros:</strong> {artista.associatedGenres?.map(g => g.name).join(', ') || 'No disponible'}</span>
            <span><strong>Subgéneros:</strong> {artista.associatedSubgenres?.map(s => s.name).join(', ') || 'No disponible'}</span>
            <span><strong>País:</strong> {artista.originCountry || 'No disponible'}</span>
            <span><strong>Actividad:</strong> {artista.activityYears || 'No disponible'}</span>
            <span><strong>Fecha de creación:</strong> {new Date(artista.createdAt).toLocaleDateString()}</span>
          </div>
          <p><strong>Biografía:</strong> {artista.biography || 'No disponible'}</p>
        </div>
      </div>

      {/* Tabs */}
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

      {/* Contenido de tabs */}
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
          {/* Selector para enviar nueva calificación */}
          <div className="d-flex mb-3">
            {[1, 2, 3, 4, 5].map(n => (
              <span
                key={n}
                style={{ cursor: 'pointer', color: n <= calificacion ? 'gold' : 'gray', fontSize: '1.5rem' }}
                onClick={() => handleRating(n)}
                title={`${n} estrella${n > 1 ? 's' : ''}`}
              >★</span>
            ))}
          </div>
          <button className="btn btn-primary mb-4" onClick={enviarCalificacion}>
            Enviar Calificación
          </button>

          <h6>Calificaciones anteriores</h6>
          {calificaciones.length > 0 ? (
            <ul style={{ listStyleType: 'none', paddingLeft: 0 }}>
              {calificaciones.map((r, i) => (
                <li key={i} style={{ marginBottom: '0.5rem' }}>
                  <strong>{r.username}</strong>: {mostrarEstrellas(r.calificacion)}
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
