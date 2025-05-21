import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Swal from 'sweetalert2';

const VerCluster = () => {
  const { id } = useParams();
  const [data, setData] = useState([]);
  const navigate = useNavigate();

  return (
    <div className='d-flex flex-column justify-content-center align-items-center bg-light vh-100'>
      <h1>Clusters creados</h1>
      <div className='w-75 rounded bg-white border shadow p-4'>

        {/* Botón para crear nuevo clúster */}
        <div className="mb-3 d-flex justify-content-end">
          <Link to="/curador/crearcluster" className="btn btn-success">
            Crear Clúster
          </Link>
        </div>

        <table className='table table-striped'>
          <thead>
            <tr>
              <th>Identificador</th>
              <th>Nombre</th>
              <th>Estado</th>
              <th>Fecha de creación</th>
            </tr>
          </thead>
        </table>
      </div>
    </div>
  );
}

export default VerCluster;
