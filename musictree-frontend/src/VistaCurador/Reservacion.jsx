import React, { useEffect, useState } from 'react'
import Swal from 'sweetalert2';
import { Link, useNavigate } from 'react-router-dom';
const Reservacion = () => {
    const [date, setDate] = useState('');
    const [data, setData] = useState([]);
  
    const handleDateChange = (event) => {
      const selectedDate = event.target.value;
      setDate(selectedDate);
      fetchData(selectedDate);
    };
  
    const fetchData = async (selectedDate) => {
      const formattedDate = selectedDate.replace(/-/g, '/'); // Formatear la fecha como 'yyyy/MM/dd'
      const url = `http://localhost:5197/api/Cama/camas_disponibles?fecha=${encodeURIComponent(formattedDate)}`;
  
      try {
        const response = await fetch(url);
        const result = await response.json();
        const uniqueData = removeDuplicates(result);
        setData(uniqueData);
      } catch (error) {
        console.error('Error fetching data:', error);
      }
    };
  
    const removeDuplicates = (data) => {
      const unique = [];
      const seen = new Set();
  
      data.forEach(item => {
        if (!seen.has(item.numcama)) {
          unique.push(item);
          seen.add(item.numcama);
        }
      });
  
      return unique;
    };
  
    return (
        <div className='d-flex flex-column justify-content-center align-items-center bg-light vh-100'>
            <h1>Consulta de camas disponibles</h1>
            <input type="date" value={date} onChange={handleDateChange} />
          <div className='w-75 rounded bg-white border shadow p-4'>
            <table className='table table-striped'>
              <thead>
                <tr>
                  <th>Numero de cama</th>
                  <th>Numero de salon</th>
                  <th>UCI</th>
                  <th>Agendar</th>
                </tr>
              </thead>
              <tbody>
                {
                  //Mapea la informacion con las filas
                  data.map((d, i) =>(
                    <tr key={i}>
                      <td>{d.numcama}</td>
                      <td>{d.num_salon}</td>
                      <td>{d.uci ? 'Si' : 'No'}</td>
                      <td> <Link to={`/paciente/reservacion/registrar/${d.numcama}`}  className='btn btn-sm btn-primary me-2'><i className='fa-solid fa-calendar'></i></Link></td>
                    </tr>
                  ))
                }
              </tbody>
            </table>
      
          </div>
          
      
        </div>
      )
  };
  
    

export default Reservacion