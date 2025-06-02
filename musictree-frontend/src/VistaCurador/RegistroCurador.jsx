import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';

const RegistroCurador = () => {
  const [values, setValues] = useState({
    nombre: '',
    ap1: '',
    ap2: '',
    cedula: '',
    nacimiento: null,
    direccion: '',
    correo: '',
    password: '',
  });

  const [dynamicFields, setDynamicFields] = useState([{ id_patologia: '', tratamiento: '' }]);
  const [procedimientos, setProcedimientos] = useState([]);

  const navigate = useNavigate();


  useEffect(() => {
    axios.get('https://localhost:7061/api/Patologia/patologias')
      .then(res => {
        setProcedimientos(res.data);
      })
      .catch(err => console.log(err));
  }, []);
  const handleSubmit = async (event) => {
    event.preventDefault();

    const formattedDynamicFields = dynamicFields.map(field => ({
      id_patologia: field.id_patologia,
      cedulapaciente: values.cedula,
      tratamiento: field.tratamiento,
    }));

    try {
      // Primero registramos el paciente
      await axios.post('http://localhost:5197/api/Paciente/registrar_paciente', values);

      // Luego registramos cada patología del paciente
      for (const field of formattedDynamicFields) {
        await axios.post('https://localhost:7061/api/Paciente/registrar_patologia_por_paciente', field);
      }

      console.log('Paciente y patologías registradas correctamente');
      navigate('/loginpaciente');
    } catch (err) {
      console.log(err);
    }
  };

  

  const addDynamicField = () => {
    setDynamicFields([...dynamicFields, { id_patologia: '', tratamiento: '' }]);
  };

  const handleDynamicFieldChange = (index, event) => {
    const { name, value } = event.target;
    const newDynamicFields = dynamicFields.map((field, i) => {
      if (i === index) {
        return { ...field, [name]: value };
      }
      return field;
    });
    setDynamicFields(newDynamicFields);
  };

  return (
    <div className='d-flex w-100 vh-100 justify-content-center align-items-center bg-light'>
      <div className='w-50 border bg-white shadow px-5 pt-3 pb-5 rounded'>
        <h1>REGISTRO DE PACIENTE</h1>
        <form onSubmit={handleSubmit}>
          <div className='mb-2'>
            <label htmlFor='nombre'>Nombre</label>
            <input type='text' name='nombre' className='form-control' placeholder='Ingrese el nombre'
              onChange={e => setValues({ ...values, nombre: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='ap1'>Primer Apellido</label>
            <input type='text' name='ap1' className='form-control' placeholder='Ingrese su primer apellido'
              onChange={e => setValues({ ...values, ap1: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='ap2'>Segundo Apellido</label>
            <input type='text' name='ap2' className='form-control' placeholder='Ingrese su segundo apellido'
              onChange={e => setValues({ ...values, ap2: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='cedula'>Cedula</label>
            <input type='number' name='cedula' className='form-control' placeholder='Ingrese su cedula'
              onChange={e => setValues({ ...values, cedula: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='password'>Contrasena</label>
            <input type='text' name='password' className='form-control' placeholder='Ingrese una contrasena'
              onChange={e => setValues({ ...values, password: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='direccion'>Direccion</label>
            <input type='text' name='direccion' className='form-control' placeholder='Ingrese su direccion'
              onChange={e => setValues({ ...values, direccion: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='correo'>Correo</label>
            <input type='text' name='correo' className='form-control' placeholder='Ingrese su correo'
              onChange={e => setValues({ ...values, correo: e.target.value })} />
          </div>
          <div className='mb-2'>
            <label htmlFor='nacimiento'>Fecha de nacimiento</label>
            <input type='date' name='nacimiento' className='form-control' placeholder='Ingrese su fecha de nacimiento'
              onChange={e => setValues({ ...values, nacimiento: e.target.value })} />
          </div>

          {dynamicFields.map((field, index) => (
            <div key={index} className='mb-2'>
              <label htmlFor={`id_patologia-${index}`}>Patología</label>
              <select name='id_patologia' className='form-control' value={field.id_patologia}
                onChange={e => handleDynamicFieldChange(index, e)}>
                <option value=''>Seleccione una patololgía</option>
                {procedimientos.map(proc => (
                  <option key={proc.id} value={proc.id}>
                    {proc.nombre}
                  </option>
                ))}
              </select>
              <label htmlFor={`tratamiento-${index}`}>Tratamiento</label>
              <input type='text' name='tratamiento' className='form-control' placeholder='Ingrese el tratamiento'
                value={field.tratamiento} onChange={e => handleDynamicFieldChange(index, e)} />
                
            </div>
          ))}
          <div>
          <button type='button' className='btn btn-secondary' onClick={addDynamicField}>Agregar otra patología</button>

          </div>
          
          <h1></h1>

          <button type='submit' className='btn btn-success'>REGISTRAR</button>
          <Link to="/logincurador" className='btn btn-primary ms-3'>VOLVER</Link>
        </form>
      </div>
    </div>
  );
};

export default RegistroCurador;
