import React from "react";
import { useNavigate } from "react-router-dom";

const opciones = [
  { name: "Buscar Artistas", path: "/fanaticos/buscarartistafanaticos" }
];

const MenuFanaticos = () => {
  const navigate = useNavigate();

  return (
    <div className="container text-center mt-5">
      <h2>Menú de los Fanáticos</h2>
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

export default MenuFanaticos;
