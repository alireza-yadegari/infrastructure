namespace Common.Configuration.Dto;

public record EmailTemplateVariables(string CompanyName);
public record EmailTemplate(string FilePath, EmailTemplateVariables Variables);