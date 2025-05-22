import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import Swal from 'sweetalert2';

const api = import.meta.env.VITE_API_URL;

const VerCluster = () => {
  const [data, setData] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    axios.get(`${api}/api/Clusters?includeInactive=true`)
      .then(res => {
        setData(res.data);
      })
      .catch(err => {
        console.error("Error al obtener clústeres:", err);
        Swal.fire("Error", "No se pudieron cargar los clústeres.", "error");
      });
  }, []);

  return (
    <div className='d-flex flex-column align-items-center bg-light min-vh-100 py-4'>
      <div className='w-75 border bg-white shadow px-5 pt-3 pb-5 rounded'>
        <h1 className='mb-4'>Lista de Clústeres</h1>

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
            {data.map(cluster => (
              <tr key={cluster.id}>
                <td>{cluster.id}</td>
                <td>{cluster.name}</td>
                <td>{cluster.isActive ? 'Activo' : 'Inactivo'}</td>
                <td>{new Date(cluster.creationDate).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <Link to="/curador/menucurador" className='btn btn-primary'>VOLVER</Link>
      </div>
    </div>
  );
};

export default VerCluster;