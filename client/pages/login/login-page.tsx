import { useUnit } from 'effector-react';
import { $email, $password, submitLogin, updateEmail, updatePassword } from './model';

export const LoginPage = () => {
  return <SignIn />;
};

const SignIn = () => {
  const [password, updatePasswordCall, email, updateEmailCall, submitLoginCall] = useUnit([
    $password,
    updatePassword,
    $email,
    updateEmail,
    submitLogin,
  ]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', marginTop: '50px' }}>
      <h2>Sign In</h2>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          submitLoginCall();
        }}
        style={{ maxWidth: '400px', margin: 'auto' }}
      >
        <label htmlFor="email">Email</label>
        <input
          type="email"
          id="email"
          value={email ?? ''}
          onChange={(e) => updateEmailCall(e.target.value)}
          placeholder="Enter your email"
          required
        />

        <label htmlFor="password">Password</label>
        <input
          type="password"
          id="password"
          value={password ?? ''}
          onChange={(e) => updatePasswordCall(e.target.value)}
          placeholder="Enter your password"
          required
        />

        <button type="submit" className="contrast">
          Sign In
        </button>
      </form>
    </div>
  );
};
