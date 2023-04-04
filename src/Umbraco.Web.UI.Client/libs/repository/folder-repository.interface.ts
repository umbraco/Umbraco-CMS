import type {
	CreateFolderRequestModel,
	FolderModelBaseModel,
	FolderReponseModel,
	ProblemDetailsModel,
	UpdateFolderReponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderRepository {
	createFolderScaffold(parentId: string | null): Promise<{
		data?: FolderReponseModel;
		error?: ProblemDetailsModel;
	}>;
	createFolder(folderRequest: CreateFolderRequestModel): Promise<{
		data?: string;
		error?: ProblemDetailsModel;
	}>;

	requestFolder(unique: string): Promise<{
		data?: FolderReponseModel;
		error?: ProblemDetailsModel;
	}>;

	updateFolder(
		unique: string,
		folder: FolderModelBaseModel
	): Promise<{
		data?: UpdateFolderReponseModel;
		error?: ProblemDetailsModel;
	}>;

	deleteFolder(id: string): Promise<{
		error?: ProblemDetailsModel;
	}>;
}
