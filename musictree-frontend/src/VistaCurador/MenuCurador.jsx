import React from "react";
import { useNavigate } from "react-router-dom";

const roles = [
{ name: "Crear Clúster", path: "/crearcluster" },
{ name: "Ver Clústers", path: "/vercluster" },
{ name: "Crear Género/Subgénero", path: "/creargenero" }
];

const MenuCurador = () => {
const navigate = useNavigate();

    return (
        <div className="container text-center mt-5">
            <h2>Seleccione su tipo de usuario</h2>
                <div className="row justify-content-center mt-4">
                {roles.map((role, index) => (
                <div className="col-md-4 mb-3" key={index}>
                <button
                    className="btn btn-primary w-100"
                    onClick={() => navigate(role.path)}
                >
                {role.name}
                </button>
                </div>
                ))}
            </div>
        </div>
    );
};

export default MenuCurador;