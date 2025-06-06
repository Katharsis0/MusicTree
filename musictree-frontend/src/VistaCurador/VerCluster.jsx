import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const VerCluster = () => {
  const [data, setData] = useState([]);
  const [soloActivos, setSoloActivos] = useState(false);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    setCargando(true);
    axios.get(`${api}/api/Clusters?includeInactive=true`)
      .then(res => {
        console.log("Datos recibidos del backend:", res.data);  
        setData(res.data || []);
        setCargando(false);
      })
      .catch(err => {
        console.error("Error al obtener clústeres:", err);
        Swal.fire("Error", "No se pudieron cargar los clústeres.", "error");
        setCargando(false);
      });
  }, []);

  const clustersFiltrados = soloActivos
    ? data.filter(c => c.isActive ?? c.IsActive)
    : data;

  return (
    <div className='d-flex flex-column align-items-center bg-light min-vh-100 py-4'>
      <div className='w-75 border bg-white shadow px-5 pt-3 pb-5 rounded'>
        <h1 className='mb-4'>Lista de Clústeres</h1>

        <div className='d-flex justify-content-between align-items-center mb-3'>
          <button
            className={`btn ${soloActivos ? 'btn-outline-secondary' : 'btn-secondary'}`}
            onClick={() => setSoloActivos(!soloActivos)}
          >
            {soloActivos ? 'Mostrar Todos' : 'Mostrar Solo Activos'}
          </button>
          <Link to="/curador/menucurador" className='btn btn-primary'>VOLVER</Link>
        </div>

        {cargando ? (
          <div className="text-center text-muted">Cargando clústeres...</div>
        ) : (
          <table className='table table-striped'>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Estado</th>
                <th>Fecha de creación</th>
              </tr>
            </thead>
            <tbody>
              {clustersFiltrados.length === 0 ? (
                <tr>
                  <td colSpan="4" className="text-center text-muted">
                    No hay clústeres {soloActivos ? 'activos' : 'registrados'}.
                  </td>
                </tr>
              ) : (
                clustersFiltrados.map(cluster => (
                  <tr key={cluster.id ?? cluster.Id}>
                    <td>{cluster.id ?? cluster.Id}</td>
                    <td>{cluster.name ?? cluster.Name}</td>
                    <td>{(cluster.isActive ?? cluster.IsActive) ? 'Activo' : 'Inactivo'}</td>
                    <td>{cluster.creationDate ? new Date(cluster.creationDate).toLocaleDateString() : 
                         cluster.timeStamp ? new Date(cluster.timeStamp).toLocaleDateString() : '-'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default VerCluster;
