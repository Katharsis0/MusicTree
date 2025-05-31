import React,{useEffect, useState} from 'react'
import { useNavigate, Link, Navigate } from 'react-router-dom';

import DivInput from '../Components/DivInput';



const LoginCurador = () => {
    const [data, setData] = useState([]);

    //Se guarda el corre y password para enviarlo a la api
    const [values, setValues] = useState({
        correo: '',
        password: '',
      })
   
    const go = useNavigate();
    useEffect(() => {
    
            <Navigate to='/profesoresr' />
  
    }, [])
  
    const handleSubmit = (event) =>{
      event.preventDefault();

    }
    const login = async(e) =>{
      e.preventDefault();
  /*    
   //------------------------------------ESTO HAY  QUE MODIFICARLO----------------------
      //Si se validan los imprime un mensaje en consola
      axios.post('http://localhost:5197/api/Login/login_pacientes',values)
      .then(res => setData(res.data),  localStorage.setItem('user-info', true), localStorage.setItem('data-paciente', JSON.stringify(data)))
      .then(() => {
        console.log("Hizo loggggg");
       // go('/paciente/reservacion')
      })
      .catch(err => console.log(err));

      axios.post('http://localhost:5197/api/Login/login_pacientes',values)
      .then(res => localStorage.setItem('data-paciente', JSON.stringify(res.data)))
      .then(() => {
        localStorage.setItem('userPaciente', true)
        go('/curador/reservacion')
      })
      .catch(err => console.log(err));
*/
       go('/curador/menucurador') 
  
    }
  
    
    //Formulario y botones
    return (
      <div className='container-fluid'>
        <div className='row mt-5'>
          <div className='col-md-4 offset-md-4'>
            <div className='card border border-dark'>
              <div className='card-header bg-dark border border-dark 
              text-white'>
                LOGIN CURADOR
              </div>
              <div className='card-body'>
                <form onSubmit={login}>
                  <DivInput type='email' icon='fa-at' value={values.correo}
                  className='form-control' placeholder='Email' required='required'
                  handleChange={(e)=> setValues({...values,correo: e.target.value})} />
                  <DivInput type='password' icon='fa-key' value={values.password}
                  className='form-control' placeholder='Password' required='required'
                  handleChange={(e)=> setValues({...values,password: e.target.value})} />
                  <div className='d-grid col-10 mx-auto'>
                    <button className='btn btn-dark'>
                      <i className='fa-solid fa-door-open'></i>
                      LOGIN
                    </button>
                  </div>
                </form>
                <Link to='/registercurador'>
                  <i className='fa-solid fa-user-plus'></i>REGISTRO
                </Link>
              </div>
            </div>  
          </div>
        </div>
      </div>
    )
  }

export default LoginCurador 