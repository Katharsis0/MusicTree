import React from "react";
import { useNavigate } from "react-router-dom";

const opciones = [
  { name: "Crear Clúster", path: "/curador/crearcluster" },
  { name: "Ver Clústers", path: "/curador/vercluster" },
  { name: "Crear Género/Subgénero", path: "/curador/creargenero" },
  { name: "Importar Géneros", path: "/curador/importargeneros" },
  { name: "Crear Artista", path: "/curador/crearartista" },
  { name: "Ver Catálogo de Artistas", path: "/curador/catalogoartistas" }
];

const MenuCurador = () => {
  const navigate = useNavigate();

  return (
    <div className="container text-center mt-5">
      <h2>Menú del Curador</h2>
      <div className="row justify-content-center mt-4">
        {opciones.map((opcion, index) => (
          <div className="col-md-4 mb-3" key={index}>
            <button
              className="btn btn-primary w-100"
              onClick={() => navigate(opcion.path)}
            >
              {opcion.name}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MenuCurador;
