import React from "react";
import "bootstrap-icons/font/bootstrap-icons.css";

const Sidebar = () => {
  return (
    <div className="sidebar d-flex flex-column justify-content-between bg-dark text-white p-4 vh-100">
      <div>
        <a href="d-flex align-items-center">
          <i className="bi bi-bootstrap fs-5 me-2"></i>
          <span className="fs-4">Sidebar</span>
        </a>
        <hr className="text-secondary mt-2" />
        <ul className="nav nav-pills flex-column p-0 m-0">
          <li className="nav-item p-1">
            <Link to='/loginpaciente' className="nav-link text-white">
              <i className="bi bi-speedometer me-2 fs-5"></i>
              <span className="fs-5">Dashboard</span>
            </Link>
          </li>
          <li className="nav-item p-1">
            <a href="" className="nav-link text-white">
              <i className="bi bi-speedometer me-2 fs-5"></i>
              <span className="fs-5">Reservacion</span>
            </a>
          </li>
          <li className="nav-item p-1">
            <a href="" className="nav-link text-white">
              <i className="bi bi-speedometer me-2 fs-5"></i>
              <span className="fs-5">Historial</span>
            </a>
          </li>
          <li className="nav-item p-1">
            <a href="" className="nav-link text-white">
              <i className="bi bi-speedometer me-2 fs-5"></i>
              <span className="fs-5">Evaluacion</span>
            </a>
          </li>
        </ul>
      </div>
      <div>
        <hr className="text-secondary" />
        <i className="bi bi-person fs-5"></i>
        <span className="fs-4">YOUR USER</span>
      </div>
    </div>
  );
};

export default Sidebar;
