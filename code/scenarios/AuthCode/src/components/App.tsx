import { Route, Routes } from 'react-router-dom';
import { appRoutes } from '../constants.ts';
import { Home } from './routes/Home.tsx';
import { NotFound } from './routes/NotFound.tsx';

export const App: React.FC = () => {
  return (
    <Routes>
      <Route path={appRoutes.home}>
        <Route index={true} element={<Home />} />
        <Route path={appRoutes.notFound} element={<NotFound />} />
      </Route>
    </Routes>
  );
};
