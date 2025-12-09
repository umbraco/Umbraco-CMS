export const UMB_MOCK_SET_NAME = import.meta.env.VITE_MOCK_SET || 'default';

const loadSet = async () => {
	switch (UMB_MOCK_SET_NAME) {
		case 'test':
			return import('./test/index.js');
		default:
			return import('./default/index.js');
	}
};

export const dataSet = await loadSet();

console.log(`[MSW] Using mock data set: "${UMB_MOCK_SET_NAME}"`);
