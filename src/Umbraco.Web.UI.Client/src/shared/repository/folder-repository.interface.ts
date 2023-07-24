import type {
	CreateFolderRequestModel,
	FolderModelBaseModel,
	FolderReponseModel,
	ProblemDetails,
	UpdateFolderReponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderRepository {
	createFolderScaffold(parentId: string | null): Promise<{
		data?: FolderReponseModel;
		error?: ProblemDetails;
	}>;
	createFolder(folderRequest: CreateFolderRequestModel): Promise<{
		data?: string;
		error?: ProblemDetails;
	}>;

	requestFolder(unique: string): Promise<{
		data?: FolderReponseModel;
		error?: ProblemDetails;
	}>;

	updateFolder(
		unique: string,
		folder: FolderModelBaseModel
	): Promise<{
		data?: UpdateFolderReponseModel;
		error?: ProblemDetails;
	}>;

	deleteFolder(id: string): Promise<{
		error?: ProblemDetails;
	}>;
}
