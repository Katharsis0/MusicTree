import React, { useState } from 'react';
import axios from 'axios';
import Swal from 'sweetalert2';
import { Link } from 'react-router-dom';

const api = import.meta.env.VITE_API_URL;

const ImportarGeneros = () => {
  const [archivo, setArchivo] = useState(null);
  const [procesando, setProcesando] = useState(false);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file && file.type === 'application/json') {
      setArchivo(file);
    } else {
      Swal.fire('Error', 'Debe seleccionar un archivo JSON válido.', 'error');
      setArchivo(null);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!archivo) {
      Swal.fire('Error', 'Debe seleccionar un archivo antes de importar.', 'warning');
      return;
    }

    const formData = new FormData();
    formData.append('archivo', archivo);

    try {
      setProcesando(true);

      const response = await axios.post(`${api}/api/Genres/importar`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      const { totalImportados, totalErrores, nombreArchivoErrores } = response.data;

      let mensaje = `Importación finalizada.\n\nTotal de géneros importados: ${totalImportados}\nErrores: ${totalErrores}`;
      if (totalErrores > 0 && nombreArchivoErrores) {
        mensaje += `\n\nEl archivo de errores se guardó como: ${nombreArchivoErrores}`;
      }

      Swal.fire('Proceso completado', mensaje, 'success');
      setArchivo(null);
    } catch (error) {
      console.error(error);
      Swal.fire('Error', 'Ocurrió un error al importar los géneros. Intente más tarde.', 'error');
    } finally {
      setProcesando(false);
    }
  };

  return (
    <div className="container py-4">
      <h1>Importar Géneros desde JSON</h1>

      <form onSubmit={handleSubmit} className="mt-4">
        <div className="mb-3">
          <label className="form-label">Archivo JSON</label>
          <input type="file" accept=".json" className="form-control" onChange={handleFileChange} />
        </div>

        <div className="d-flex gap-3">
          <button type="submit" className="btn btn-success" disabled={procesando}>
            {procesando ? 'Procesando...' : 'Importar'}
          </button>
          <Link to="/curador/menucurador" className="btn btn-primary">Volver</Link>
        </div>
      </form>

      <div className="alert alert-info mt-4">
        <strong>Nota:</strong> El archivo debe tener un arreglo de géneros con las propiedades correctas. Si hay errores en registros individuales, se generará un archivo con los errores específicos.
      </div>
    </div>
  );
};

export default ImportarGeneros;
