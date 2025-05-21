import React, { useEffect, useState } from 'react'
import axios from 'axios';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Swal from 'sweetalert2';
const VerReservaciones = () => {
    const { id } = useParams(); // Obtiene el ID del activo de los parámetros de la URL
    const [data, setData] = useState([]);
    const navigate = useNavigate();
  
    useEffect(()=>{
    const pacienteData = JSON.parse(localStorage.getItem('data-paciente'));
    const pacienteCedula = pacienteData ? pacienteData.cedula : '';
      axios.get('http://localhost:5197/api/Reservacion/reservaciones/'+pacienteCedula) //MODIFICAR ESTO para eleigfir bien la cedula
      .then(res => setData(res.data))
      .catch(err => console.log(err));
    },[]);
  
  
  // Función para manejar la eliminación de un laboratorio
    function handleDelete(nombre) {
      Swal.fire({
        title: '¿Está seguro que desea elminar esta reservacion?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminarlo'
      }).then((result) => {
        if (result.isConfirmed) {
          axios.delete(`http://localhost:5197/api/Reservacion/eliminar_reservacion/${nombre}`)
            .then((res) => {
              Swal.fire(
                '¡Eliminado!',
                'La reservacion ha sido eliminada.',
                'success'
              );
              location.reload(); // Este es un método poco elegante, puedes manejar el refresco de datos de una manera más eficiente dependiendo de tu flujo de datos.
            })
            .catch((err) => {
              console.log(err);
              Swal.fire(
                'Error',
                'Hubo un problema al eliminar el laboratorio.',
                'error'
              );
            });
        }
      });
    }
  
  //Tabla con la informacionde los laboratorios
  return (
    <div className='d-flex flex-column justify-content-center align-items-center bg-light vh-100'>
      <h1>Reservaciones realizadas</h1>
      <div className='w-75 rounded bg-white border shadow p-4'>
        <table className='table table-striped'>
          <thead>
            <tr>
              <th>Nombre procedimiento</th>
              <th>Numero de cama</th>
              <th>Fecha de ingreso</th>
              <th>Fecha de salida</th>
              <th>Modificar</th>
              <th>Cancelar</th>
            </tr>
          </thead>
          <tbody>
            {
              //Mapea la informacion con las filas
              data.map((d, i) =>(
                <tr key={i}>
                  <td>{d.nombre}</td>
                  <td>{d.numcama}</td>
                  <td>{d.fechaingreso}</td>
                  <td>{d.fecha_salida}</td>
                  <td> <Link to={`/paciente/reservaciones/update/${d.fechaingreso}/${d.id}`}  className='btn btn-sm btn-primary me-2'><i className='fa-solid fa-edit'></i></Link></td>
                  <td>
                    <button onClick={e => handleDelete(d.id)} className='btn btn-sm btn-danger'>
                      <i className='fa-solid fa-trash'></i>
                    </button>
                  </td>
                </tr>
              ))
            }
          </tbody>
        </table>
  
      </div>
      
  
    </div>
  )
  
  
  
  
  }

export default VerReservaciones