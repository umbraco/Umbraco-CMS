export const defaultA11yConfig = {
	ignoredRules: [],
};

export type UmbTestRunnerWindow = Window &
	typeof globalThis & {
		__UMBRACO_TEST_RUN_A11Y_TEST: boolean;
	};
