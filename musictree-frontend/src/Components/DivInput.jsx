import React,{forwardRef,useEffect,useRef} from "react";

// Exporta un componente funcional utilizando forwardRef para admitir referencias
export default forwardRef (({ 
    // Props por defecto
    type='text', 
    icon='user',
    placeholder='',
    name,
    id,
    value,
    classname,
    required,
    isFocused,
    handleChange}, 
    ref) => {

    // Utiliza useRef para crear una referencia si no se proporciona una ref
    const input = ref ? ref :useRef();

    // Utiliza useEffect para enfocar el input cuando isFocused es true
    useEffect(()=>{
        if(isFocused){
            input.current.focus();
        }
    },[]); // Se ejecuta solo cuando el componente se monta

    // Retorna el JSX del componente
    return (
        <div className="input-group mb-3">
            <span className="input-group-text">
                {/* Renderiza un ícono basado en la clase proporcionada */}
                <i className={'fa-solid '+icon}></i>
            </span>
            {/* Renderiza un input con las props proporcionadas */}
            <input 
                type={type} 
                placeholder={placeholder} 
                name={name} 
                id={id} 
                value={value} 
                className={classname} 
                ref={input} // Utiliza la ref para acceder al elemento input
                required = {required} 
                onChange={(e) => handleChange(e)} // Maneja el cambio del input
            />
        </div>
    );
});
