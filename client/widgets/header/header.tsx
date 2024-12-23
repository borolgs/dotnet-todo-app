import { Link } from 'atomic-router-react';
import { useUnit } from 'effector-react';
import React from 'react';
import { viewerQuery } from '~/shared/auth';
import { routes } from '~/shared/routing';

export const Header: React.FC = () => {
  const { data: viewer, error } = useUnit(viewerQuery);
  return (
    <nav id="nav">
      <ul>
        <li>
          <Link to={routes.home}>Home</Link>
        </li>
        <li>
          <Link to={routes.todos}>Todos</Link>
        </li>
      </ul>
      <ul>
        <li>
          {error ? (
            <form method="post" action="/auth/login">
              <input type="submit" className="outline contrast" value="Sign In" />
            </form>
          ) : (
            <details className="dropdown">
              <summary role="button" className="outline contrast">
                {viewer?.data?.email}
              </summary>
              <ul dir="rtl">
                <li>
                  <Link to={routes.profile}>Profile</Link>
                </li>
                <li onClick={() => fetch('/auth/logout', { redirect: 'manual' })}>
                  <Link to={routes.signin}>Logout</Link>
                </li>
              </ul>
            </details>
          )}
        </li>
      </ul>
    </nav>
  );
};
