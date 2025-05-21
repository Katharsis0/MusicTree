import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const RegistrarReservacion = () => {
  // Valores a enviar
  const { id } = useParams(); // Obtiene el ID del activo de los parámetros de la URL
  const [values, setValues] = useState({
    id: 1,
    fechaingreso: null,
    cedpaciente: JSON.parse(localStorage.getItem('data-paciente')).cedula,
    idprocmed: 0,
    numcama: id,
  });

  const [procedimientos, setProcedimientos] = useState([]);
  const [fechaFinal, setFechaFinal] = useState(null);
  const navigate = useNavigate();

  // Fetch procedimientos médicos desde la API cuando el componente se monte
  useEffect(() => {
    axios.get('http://localhost:5197/api/ProcedimientoMedico/procedimientos_medicos')
      .then(res => {
        setProcedimientos(res.data);
      })
      .catch(err => console.log(err));
  }, []);

  // Actualizar la fecha final cuando cambian la fecha de ingreso o el procedimiento
  useEffect(() => {
    if (values.fechaingreso && values.idprocmed) {
      const procedimiento = procedimientos.find(proc => proc.id === parseInt(values.idprocmed));
      if (procedimiento) {
        const fechaIngreso = new Date(values.fechaingreso);
        const fechaFinalCalculada = new Date(fechaIngreso);
        fechaFinalCalculada.setDate(fechaIngreso.getDate() + procedimiento.cantdias);
        setFechaFinal(fechaFinalCalculada);
      }
    }
  }, [values.fechaingreso, values.idprocmed, procedimientos]);

  // Funcion al presionar aceptar
  const handleSubmit = (event) => {
    event.preventDefault();
    axios.post('http://localhost:5197/api/Reservacion/registrar_reservacion', values)
      .then(res => {
        console.log(res);
        navigate('/paciente/reservacion');
      })
      .catch(err => console.log(err));
  };

  return (
    // Formulario para llenar los datos necesarios para crear al activo
    <div className='d-flex w-100 vh-100 justify-content-center align-items-center bg-light'>
      <div className='w-50 border bg-white shadow px-5 pt-3 pb-5 rounded'>
        <h1>AGREGAR RESERVACIÓN</h1>
        <form onSubmit={handleSubmit}>
          <div className='mb-2'>
            <label htmlFor='fechaingreso'>Fecha de ingreso</label>
            <input type='date' name='fechaingreso' className='form-control' placeholder='Ingreso'
              onChange={e => setValues({ ...values, fechaingreso: e.target.value })} />
          </div>

          <div className='mb-2'>
            <label htmlFor='idprocmed'>Procedimiento Médico</label>
            <select name='idprocmed' className='form-control' onChange={e => setValues({ ...values, idprocmed: e.target.value })}>
              <option value=''>Seleccione un procedimiento</option>
              {procedimientos.map(proc => (
                <option key={proc.id} value={proc.id}>
                  {proc.nombre}
                </option>
              ))}
            </select>
          </div>
          {fechaFinal && (
            <div className='mb-2'>
              <label htmlFor='fechafinal'>Fecha Final</label>
              <input type='text' name='fechafinal' className='form-control' readOnly value={fechaFinal.toISOString().split('T')[0]} />
            </div>
          )}
          <button className='btn btn-success'>GUARDAR</button>
          <Link to="/activos" className='btn btn-primary ms-3'>VOLVER</Link>
        </form>
      </div>
    </div>
  );
};

export default RegistrarReservacion;
