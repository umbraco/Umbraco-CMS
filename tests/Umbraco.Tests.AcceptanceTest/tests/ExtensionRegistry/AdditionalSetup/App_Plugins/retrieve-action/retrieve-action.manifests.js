export const manifest = {
  type: 'entityAction',
  kind: 'default',
  alias: 'Retrieve',
  name: 'Retrieve',
  weight: 200,
  forEntityTypes: ['document', 'media'],
  api: () => import('./retrieve-action.api.js'),
  meta: {
    icon: 'icon-add',
    label: 'Retrieve'
  },
};