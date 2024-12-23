export default (api) => {
  api.cache(true);
  return {
    presets: [['@babel/preset-env', { targets: { node: 'current' } }], '@babel/preset-typescript'],
    plugins: ['effector/babel-plugin'],
  };
};
