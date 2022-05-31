import { rest } from 'msw';
import { umbContentData } from '../data/content.data';


// TODO: add schema
export const handlers = [
  rest.get('/umbraco/backoffice/content/:id', (req, res, ctx) => {
    const id = req.params.id as string;
    if (!id) return;
    
    const int = parseInt(id);
    const document = umbContentData.getById(int);
    
    return res(
      ctx.status(200),
      ctx.json([document])
    );
  }),

  rest.post('/umbraco/backoffice/content/save', (req, res, ctx) => {
    const data = req.body as any;
    if (!data) return;


    umbContentData.save(data);

    return res(
      ctx.status(200),
      ctx.json(data)
    );
  }),
];